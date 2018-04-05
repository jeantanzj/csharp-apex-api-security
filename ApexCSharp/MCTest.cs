using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using ApiUtilLib;
using Newtonsoft.Json;

namespace ApexCSharp
{
    public class MCTest
    {
        const string _proxySigningHost = "";
        const string _proxyTargetHost = "";
        const string _sourceSigningHost = "";
        const string _sourceTargetHost = "";
        const string _awsHost = "";
        private static System.Collections.Specialized.NameValueCollection _settings = ConfigurationManager.AppSettings;
        private static int _totalAsserts = 0;
        private static int _failedAsserts = 0;
        public static void Main(string[] args){
               
                PipedTests();
                Test_Can_SMS();
                Console.WriteLine($"_totalAsserts: { _totalAsserts} _failedAsserts: {_failedAsserts}");
        }

        static void PipedTests(){
                Test_Is_MC_Working();
                var content = Test_Can_Create_MC()?.Content.ReadAsStringAsync().Result;
                if (content != null){
                    var dict = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(content);
                    string id = "";
                    if (dict.TryGetValue("id", out id)){
                        var mcContent = Test_Can_Get_MC(id)?.Content.ReadAsStringAsync().Result;
                        var mcDict = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(mcContent);
                        string mid = "";
                        if (mcDict.TryGetValue("MedicalCertificateID", out mid)){
                            Test_Can_Void_MC(mid);
                        }
                    }
                }
        }

        static System.Net.Http.HttpResponseMessage Test_Is_MC_Working() {
            var helper = GetMCHelper("/mc",ApiUtilLib.HttpMethod.GET);
            var response = helper.Send().GetAwaiter().GetResult();
            CheckEquals(response?.StatusCode , HttpStatusCode.NoContent);
            return response;
        }

        static System.Net.Http.HttpResponseMessage  Test_Can_Create_MC(){
            var helper = GetMCHelper("/mc",ApiUtilLib.HttpMethod.POST);
            var body = GenerateCreateBody();
            var response = helper.Send(body).GetAwaiter().GetResult();
            CheckEquals(response?.StatusCode , HttpStatusCode.OK);
            return response;
        }

        static System.Net.Http.HttpResponseMessage  Test_Can_Get_MC(string id){
            var helper = GetMCHelper($"/mc/{id}",ApiUtilLib.HttpMethod.GET);
            var response = helper.Send().GetAwaiter().GetResult();
            CheckEquals(response?.StatusCode , HttpStatusCode.OK);
            return response;
        }

        static System.Net.Http.HttpResponseMessage  Test_Can_Void_MC(string id){
            var helper = GetMCHelper($"/mc/{id}",ApiUtilLib.HttpMethod.PUT);
            var body = GenerateVoidBody();
            var response = helper.Send(body).GetAwaiter().GetResult();
            CheckEquals(response?.StatusCode , HttpStatusCode.OK);
            return response;
        }
        static System.Net.Http.HttpResponseMessage Test_Can_SMS(){
            var endpoint = "/sms";
            var proxy = new Gateway (_settings, GatewayType.PROXY, "L1",
                    signingUrlPath: $"{_awsHost}{endpoint}", 
                    targetUrlPath: $"{_awsHost}{endpoint}");
            var helper = new MCHelper(GatewayType.PROXY, proxy, null, ApiUtilLib.HttpMethod.POST);
            var body = GenerateSMSBody();
            var response = helper.Send(body).GetAwaiter().GetResult();
            CheckEquals(response?.StatusCode , HttpStatusCode.OK);
            return response;
        }


         static MCHelper GetMCHelper (string endpoint, ApiUtilLib.HttpMethod httpMethod){
            var proxy = new Gateway (_settings, GatewayType.PROXY, "L2",
                signingUrlPath: $"{_proxySigningHost}{endpoint}", 
                targetUrlPath: $"{_proxyTargetHost}{endpoint}");
            var source = new Gateway (_settings, GatewayType.SOURCE, "L1", 
                signingUrlPath: $"{_sourceSigningHost}{endpoint}",
                targetUrlPath: $"{_sourceTargetHost}{endpoint}");
            return new MCHelper(GatewayType.PROXY, proxy, source, httpMethod);
        }

        static string GenerateCreateBody(string id = null){
            id = id ?? Guid.NewGuid().ToString();
            var time = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK");
            return "{\"CreatedByName\": \"MCTest\", \"CreatedWhen\": \""+time+"\", \"ProviderMCR\": \"string\", \"PatientID\": \"string\", \"PatientName\": \"string\", \"PhoneNumber\": \"string\", \"Institution\": \"Singapore General Hospital\", \"VisitID\": \""+id+"\", \"AdmitDatetime\": \"\", \"DischargeDatetime\": \"\", \"Department\": \"string\", \"Ward\": \"string\", \"Status\": \"Active\", \"MedicalCertificateID\": \""+id+"\", \"FlagUnfit\": true, \"FlagFit\": true, \"FlagCourt\": true, \"FlagDuration\": true, \"MCType\": 0, \"SignificantDtm\": \"\", \"UnfitFrom\": \"\", \"UnfitTo\": \"\", \"FitFrom\": \"\", \"FitTo\": \"\", \"DurationFrom\": \"\", \"DurationTo\": \"\", \"DutyComment\": \"\", \"Diagnosis\": \"\", \"SurgicalOperations\": \"\", \"VoidBy\": \"\", \"VoidWhen\": \"\", \"KKHDeliveryDate\": \"\"}";
            
        }
        static string GenerateVoidBody(string name = null){
            var time = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK");
            return "{ \"VoidBy\": \"MCTest\", \"VoidWhen\": \""+time+"\"}";
        }

        static string GenerateSMSBody(string id = null, string phoneNumber=null){
            id = id ?? Guid.NewGuid().ToString();
            phoneNumber = phoneNumber ?? "00000000";
            return "{\"id\": \""+id+"\", \"phoneNumber\": \""+phoneNumber+"\"}";
        }

        static bool CheckEquals(object a, object b, [System.Runtime.CompilerServices.CallerMemberName] string callerMember= ""){
            _totalAsserts += 1;
            var check = a.Equals(b);
            _failedAsserts += (check ? 0 : 1);
            if(!check) Console.WriteLine($"{callerMember} - FAILED - {a.ToString()} != {b.ToString()}");
            return check;
        }
    }
}
