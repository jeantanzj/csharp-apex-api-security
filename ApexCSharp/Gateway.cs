using System;
using System.IO;
using ApiUtilLib;
namespace ApexCSharp{
    public class Gateway {
        private GatewayType _type;
        private string _appId;
        private string _appSecret;
        private string _certFileName;
        private string _certPassPhrase;
        private string _realm;
        private string _signingUrlPath;
        private string _targetUrlPath;
        private string _auth;
        public Gateway(System.Collections.Specialized.NameValueCollection _settings, GatewayType type, string auth, string signingUrlPath, string targetUrlPath){
            this._type = type;
            this._auth = auth;
            this._signingUrlPath = signingUrlPath;
            this._targetUrlPath = targetUrlPath;
            this._appId = (_type == GatewayType.PROXY ? _settings["proxy.app_id"] : _settings["source.app_id"] ) ?? "Not found";
            this._appSecret = (_type == GatewayType.PROXY ? _settings["proxy.app_secret"] : _settings["source.app_secret"] ) ?? "Not found";
            this._certFileName = (_type == GatewayType.PROXY ? _settings["proxy.cert_file_name"] : _settings["source.cert_file_name"] ) ?? "Not found";
            this._certPassPhrase = (_type == GatewayType.PROXY ? _settings["proxy.cert_pass_phrase"] : _settings["source.cert_pass_phrase"] ) ?? "Not found";
            this._realm = _settings["realm"] ?? "Not found";
        }
        
        public string TargetUrl {
            get { return _targetUrlPath; }
            set { if (value != _targetUrlPath) _targetUrlPath = value; }
        }
        public string SigningUrl {
            get { return _signingUrlPath; }
            set { if (value != _signingUrlPath) _signingUrlPath = value; }
        }


        public string GetSignature(ApiUtilLib.HttpMethod httpMethod){
            if(string.IsNullOrEmpty(_auth)){
                return "";
            }
            return _auth.ToUpper() == "L2" ? SignL2(httpMethod) : SignL1(httpMethod);   
          
        }
        string SignL1(ApiUtilLib.HttpMethod httpMethod){
            var authPrefix = _type == GatewayType.PROXY ? "Apex_l1_eg" : "Apex_l1_ig";
            return ApiAuthorization.Token(realm:  _realm, authPrefix: authPrefix, httpMethod: httpMethod, 
            urlPath: new Uri(_signingUrlPath), appId: _appId, secret: _appSecret);
        }

        string SignL2(ApiUtilLib.HttpMethod httpMethod){
            var authPrefix = _type == GatewayType.PROXY ? "Apex_l2_eg" : "Apex_l2_ig";
            var path = GetLocalPath(_certFileName);
            var privateKey = ApiAuthorization.PrivateKeyFromP12(path, _certPassPhrase);
            return ApiAuthorization.Token(realm:  _realm, authPrefix: authPrefix, httpMethod: httpMethod, 
            urlPath: new Uri(_signingUrlPath), appId: _appId, privateKey: privateKey);
        }
        static string GetLocalPath(string relativeFileName)
		{
            var localPath = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), relativeFileName.Replace('/', Path.DirectorySeparatorChar));
			return localPath;
		}

        public override string ToString() {
            //var secrets = $"_appId:{_appId}, _appSecret:{_appSecret}, _certFileName:{_certFileName}, _certPassPhrase:{_certPassPhrase}";
            var info = $"_type:{_type}, _realm:{_realm}, signingUrlPath:{_signingUrlPath}, targetUrlPath:{_targetUrlPath}, auth:{_auth}";
            return info;
        }
        

    }

    public enum GatewayType {
        PROXY,
        SOURCE
    }
}