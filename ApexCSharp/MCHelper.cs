using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApexCSharp
{
     public class MCHelper
    {
        private GatewayType _bridge;
        private Gateway _proxy;
        private Gateway _source;
        private ApiUtilLib.HttpMethod _httpMethod;
        
        private static readonly HttpClient _httpClient = new HttpClient();
      

        public MCHelper (GatewayType bridge, Gateway proxy, Gateway source, ApiUtilLib.HttpMethod httpMethod){
            this._bridge = bridge;
            this._proxy = proxy;
            this._source = source;
            this._httpMethod = httpMethod;
        }
     
        public async Task<HttpResponseMessage> Send(
            string jsonBody=""){
            var request = GetApexRequest();
            if(request!=null){ 
                var authorizationHeader = request[0];
                var targetUrlPath = request[1];
                //Console.WriteLine($"Target: {targetUrlPath} \n Header: {authorizationHeader}");
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);
                HttpResponseMessage response;
                switch(_httpMethod){
                
                    case ApiUtilLib.HttpMethod.PUT:
                        response = await _httpClient.PutAsync(new Uri(targetUrlPath),  
                        new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json"));
                        break;
                    case ApiUtilLib.HttpMethod.POST:
                        response = await _httpClient.PostAsync(new Uri(targetUrlPath),  
                            new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json"));
                        break;
                    default:
                        response = await _httpClient.GetAsync(new Uri(targetUrlPath));
                        break;
                }
                
                return response;
            }
            else{
                Console.WriteLine("GetApexRequest returned null");
                return null;
            }
        }
        
         string[] GetApexRequest(){
            string authorizationHeader = "" ;
            string targetUrlPath = "" ;
            if (_proxy != null && _source != null){
                var proxyToken = _proxy.GetSignature(_httpMethod);
                var sourceToken = _source.GetSignature(_httpMethod);
                authorizationHeader = _bridge == GatewayType.SOURCE ? Join(sourceToken, proxyToken) : Join(proxyToken, sourceToken);
                targetUrlPath = _bridge == GatewayType.SOURCE  ? _source.TargetUrl : _proxy.TargetUrl;
            }
            else if (_proxy!=null){
                authorizationHeader = _proxy.GetSignature(_httpMethod);
                targetUrlPath = _proxy.TargetUrl;
            }
            else if (_source!=null) {
                authorizationHeader = _source.GetSignature(_httpMethod);
                targetUrlPath = _source.TargetUrl;
            }
            
            if (targetUrlPath.Length > 0) {
                return new string[2] { authorizationHeader, targetUrlPath };
            }
            return null;
        }
        static string Join(string a, string b){
            return (a == "" ? "" : a + "," ) + b;
        }
        
    }

    
}
