using System;
using System.Net.Http;
using System.Threading.Tasks;
namespace ApexCSharp
{
    class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public static void  Main(string[] args)
        {
            Program.Send().GetAwaiter().GetResult();
        }
        
        static async Task Send(){
            var bridge = GatewayType.PROXY;
            var httpMethod = ApiUtilLib.HttpMethod.GET;
            var proxy = new Gateway (GatewayType.PROXY, "L1", httpMethod,
                signingUrlPath: "https://YOURPROJECT.e.api.gov.sg/NAME/v1", 
                targetUrlPath: "https://YOURPROJECT.api.gov.sg/NAME/v1");
            var source = new Gateway (GatewayType.SOURCE, "L1", httpMethod,
                signingUrlPath: "https://YOURPROJECT-pvt.i.api.gov.sg/NAME/v1", 
                targetUrlPath: "https://YOURPROJECT-pvt.api.gov.sg/NAME/v1");
            Console.WriteLine(proxy);
            Console.WriteLine(source);
            var request = GetApexRequest(bridge: bridge, proxy: proxy, source: source);
            if(request!=null){
                var authorizationHeader = request[0];
                var targetUrlPath = request[1];
                var jsonBody = request[2] ??  "";
                Console.WriteLine($"Target: {targetUrlPath} \n Header: {authorizationHeader}");
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);
                /*var response = await _httpClient.PostAsync(new Uri(targetUrlPath),  
                    new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json"));*/
                var response = await _httpClient.GetAsync(new Uri(targetUrlPath));
                Console.WriteLine(response.StatusCode);
                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            else{
                Console.WriteLine("GetApexRequest returned null");
            }
        }
        
        static string[] GetApexRequest(GatewayType bridge, Gateway proxy = null, Gateway source = null){
            string authorizationHeader = "" ;
            string targetUrlPath = "" ;
            string jsonBody = "";
            if (proxy != null && source != null){
                var proxyToken = proxy.GetSignature();
                var sourceToken = source.GetSignature();
                authorizationHeader = bridge == GatewayType.SOURCE ? Join(sourceToken, proxyToken) : Join(proxyToken, sourceToken);
                targetUrlPath = bridge == GatewayType.SOURCE  ? source.TargetUrl : proxy.TargetUrl;
                jsonBody =  bridge == GatewayType.SOURCE ? source.Body : proxy.Body;
            }
            else if (proxy!=null){
                authorizationHeader = proxy.GetSignature();
                targetUrlPath = proxy.TargetUrl;
                jsonBody  = proxy.Body;
            }
            else if (source!=null) {
                authorizationHeader = source.GetSignature();
                targetUrlPath = source.TargetUrl;
                jsonBody  = source.Body;
            }
            
            if (targetUrlPath.Length > 0) {
                return new string[3] { authorizationHeader, targetUrlPath, jsonBody };
            }
            return null;
        }
        static string Join(string a, string b){
            return (a == "" ? "" : a + "," ) + b;
        }
        
    }

    
}
