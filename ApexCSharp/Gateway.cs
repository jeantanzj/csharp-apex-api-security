using System;
using System.Configuration;
using System.IO;
using ApiUtilLib;
namespace ApexCSharp{
    class Gateway {
        private GatewayType _type;
        private ApiUtilLib.HttpMethod _httpMethod;
        private string _appId;
        private string _appSecret;
        private string _certFileName;
        private string _certPassPhrase;
        private string _realm;
        private string _signingUrlPath;
        private string _targetUrlPath;
        private string _auth;
        private string _body;
        
        private static System.Collections.Specialized.NameValueCollection _settings = ConfigurationManager.AppSettings;
        public Gateway(GatewayType type, string auth, ApiUtilLib.HttpMethod httpMethod, string signingUrlPath, string targetUrlPath, string body = null){
            this._type = type;
            this._auth = auth;
            this._httpMethod = httpMethod;
            this._signingUrlPath = signingUrlPath;
            this._targetUrlPath = targetUrlPath;
            this._body = body;
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

        public string Body {
            get { return _body; }
            set { if(value != _body) _body = value; }
        }
        public string GetSignature(){
            if(string.IsNullOrEmpty(_auth)){
                return "";
            }
            return _auth.ToUpper() == "L2" ? SignL2() : SignL1();   
          
        }
        string SignL1(){
            var authPrefix = _type == GatewayType.PROXY ? "Apex_l1_eg" : "Apex_l1_ig";
            return ApiAuthorization.Token(realm:  _realm, authPrefix: authPrefix, httpMethod: _httpMethod, 
            urlPath: new Uri(_signingUrlPath), appId: _appId, secret: _appSecret);
        }

        string SignL2(){
            var authPrefix = _type == GatewayType.PROXY ? "Apex_l2_eg" : "Apex_l2_ig";
            var path = GetLocalPath(_certFileName);
            var privateKey = ApiAuthorization.PrivateKeyFromP12(path, _certPassPhrase);
            return ApiAuthorization.Token(realm:  _realm, authPrefix: authPrefix, httpMethod: _httpMethod, 
            urlPath: new Uri(_signingUrlPath), appId: _appId, privateKey: privateKey);
        }
        static string GetLocalPath(string relativeFileName)
		{
            var localPath = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), relativeFileName.Replace('/', Path.DirectorySeparatorChar));
			return localPath;
		}

        public override string ToString() {
            //var secrets = $"_appId:{_appId}, _appSecret:{_appSecret}, _certFileName:{_certFileName}, _certPassPhrase:{_certPassPhrase}";
            var info = $"_type:{_type}, _realm:{_realm}, signingUrlPath:{_signingUrlPath}, targetUrlPath:{_targetUrlPath}, auth:{_auth}, body:{_body}";
            return info;
        }
        

    }

    enum GatewayType {
        PROXY,
        SOURCE
    }
}