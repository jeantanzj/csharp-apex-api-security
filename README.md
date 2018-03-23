# APEX API C# Security Utility 
[![Build Status](https://travis-ci.org/GovTechSG/csharp-apex-api-security.svg?branch=master)](https://travis-ci.org/GovTechSG/csharp-apex-api-security)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com)
[![MIT License](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/GovTechSG/csharp-apex-api-security/blob/master/LICENSE)

A C# helper utility that construct and sign HTTP Authorization header scheme for API authentication and verification

## Table of Contents
- [Fork changes](#fork-changes)
- [Getting Started](#getting-started)
    * [Prerequisites](#prerequisites)
    * [APIList Interface](#using-the-apilist-class)
        + [Generate QueryString](#generate-querystring)
        + [Generate FormData](#generate-formdata)
    * [Constructing HMAC256 L1 Authorization Header](#how-to-generate-hmac256-l1-authorization-header)
    * [Constructing RSA256 L2 Authorization Header](#how-to-generate-rsa256-l2-authorization-header)
- [Release](#release)
- [Contributing](#contributing)
- [License](#license)
- [References](#references)

## Fork Changes
** 2018-03-23 **
- The following changes were made to be able to build, and run in my environment

```
.NET Command Line Tools (2.1.101)

Product Information:
 Version:            2.1.101
 Commit SHA-1 hash:  6c22303bf0

Runtime Environment:
 OS Name:     Mac OS X
 OS Version:  10.13
 OS Platform: Darwin
 RID:         osx.10.12-x64
 Base Path:   /usr/local/share/dotnet/sdk/2.1.101/

Microsoft .NET Core Shared Framework Host

  Version  : 2.0.6
  Build    : 74b1c703813c8910df5b96f304b0f2b78cdf194d
```

- Cast to `RSA` instead of `RSACryptoServiceProvider`. Corresponding code and tests were modified
- Target framework was changed to `netcoreapp2.0`
- Test framework setup:
    * Microsoft.NET.Test.Sdk 15.6.1
    * NUnit 3.9.0
    * NUnit3TestAdapter 3.9.0
- To run tests: `dotnet test`

## Getting Started

### Prerequisites
+ .NET Framework 4.6.1
+ Visual Studio IDE 2015/2017 Community+
+ NUnit Framework 3.8+ (for Windows Platform)

Make sure that all unit test cases are passed before using the library.

#### Installing NUnit **(Important : Windows Only)**

For windows users , NUnitTestAdapter have to be installed before you can run the test cases succcessfully.

1.  From Tools menu, use Library Package Manager and select Manage NuGet packages for solution.

2.  In the left panel, select Online

3.  Locate (search for) NUnit 3.0 Test Adapter in the center panel and highlight it

4.  Click install, and select existing project ApiSecuritySolution to add the adapter.


### Using the ApiList Class
The ApiUtilLib Library provide the utility class ApiList to construct request Query String and Form Data.

#### Generate QueryString
```
    var queryParam = new ApiUtilLib.ApiList();

    queryParam.Add("clientId", "1256-1231-4598");
    queryParam.Add("accountStatus", "active");
    queryParam.Add("txnDate", "2017-09-29");

    string queryString = queryParam.ToQueryString();

    string baseUrl = string.Format("https://example.com/resource/?{0}", queryString);
    // https://example.com/resource/?clientId=1256-1231-4598&accountStatus=active&txnDate=2017-09-29
```

#### Generate FormData
```
    var formData = new ApiUtilLib.ApiList();

    formData.Add("phoneNo", "+1 1234 4567 890");
    formData.Add("street", "Hellowood Street");
    formData.Add("state", "AP");

    string formData = formData.ToFormData();
    // phoneNo=%2B1+1234+4567+890&street=Hellowood+Street&state=AP
```

### How to Generate HMAC256 L1 Authorization Header
```
public static void L1Sample()
{
    // application realm
    string realm = "http://example.api.com/test";

    // authorization prefix
    string authPrefix = "auth_l1";

    // app id and app secret assign to the application
    string appId = "app-id-lpX54CVNltS0ye03v2mQc0b";
    string appSecret = "5aes9wG4mQgWJBfKMuYLtrEtNslm1enWG2XpGaMk";

    // api signing gateway name and path
    string gatewayName = "example.api.com";
    string apiPath = "api/v1/l1/";

    // query string
    var queryParam = new ApiUtilLib.ApiList();

    queryParam.Add("clientId", "1256-1231-4598");
    queryParam.Add("accountStatus", "active");
    queryParam.Add("txnDate", "2017-09-29");

    string queryString = queryParam.ToQueryString();

    string baseUrl = string.Format("https://{0}/{1}?{2}", gatewayName, apiPath, queryString);

    // form data
    var formData = new ApiUtilLib.ApiList();

    formData.Add("phoneNo", "+1 1234 4567 890");
    formData.Add("street", "Hellowood Street");
    formData.Add("state", "AP");

    // authorization header
    var authorizationHeader = ApiAuthorization.Token(realm, authPrefix, HttpMethod.POST, new Uri(baseUrl), appId, appSecret, formData);

    Console.WriteLine("\n>>>Authorization Header :: '{0}'<<<", authorizationHeader);

    // if the target gateway name is different from signing gateway name
    string targetGatewayName = "example.e.api.com";
    string targetBaseUrl = string.Format("https://{0}/{1}?{2}", targetGatewayName, apiPath, queryString);

    // this method only for verification only
    // expecting result to be 200
    var result = ApiAuthorization.HttpRequest(new Uri(targetBaseUrl), authorizationHeader, formData, HttpMethod.POST, ignoreServerCert: true);
}
```

### How to Generate RSA256 L2 Authorization Header
```
public static void L2Sample()
{
    // application realm
    string realm = "http://example.api.com/test";

    // authorization prefix
    string authPrefix = "auth_l2";

    // app id assign to the application
    string appId = "app-id-lpX54CVNltS0ye03v2mQc0b";

    // api signing gateway name and path
    string gatewayName = "example.api.com";
    string apiPath = "api/v1/l2/";

    // query string
    var queryParam = new ApiUtilLib.ApiList();

    queryParam.Add("clientId", "1256-1231-4598");
    queryParam.Add("accountStatus", "active");
    queryParam.Add("txnDate", "2017-09-29");

    string queryString = queryParam.ToQueryString();

    string baseUrl = string.Format("https://{0}/{1}?{2}", gatewayName, apiPath, queryString);

    // form data
    var formData = new ApiUtilLib.ApiList();

    formData.Add("phoneNo", "+1 1234 4567 890");
    formData.Add("street", "Hellowood Street");
    formData.Add("state", "AP");

    // private cert file and password
    string privateCertName = GetLocalPath("Certificates/alpha.example.api.com.p12");
    string password = "password";

    // get the private key from cert
    var privateKey = ApiAuthorization.PrivateKeyFromP12(privateCertName, password);
    
    // authorization header
    var authorizationHeader = ApiAuthorization.Token(realm, authPrefix, HttpMethod.POST, new Uri(baseUrl), appId, null, formData, privateKey);

    // if the target gateway name is different from signing gateway name
    string targetGatewayName = "example.e.api.com";
    string targetBaseUrl = string.Format("https://{0}/{1}?{2}", targetGatewayName, apiPath, queryString);

    // this method only for verification only
    // expecting result to be 200
    var result = ApiAuthorization.HttpRequest(new Uri(targetBaseUrl), authorizationHeader, formData, HttpMethod.POST, ignoreServerCert: true);
}

static string GetLocalPath(string relativeFileName)
{
    var localPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), relativeFileName.Replace('/', Path.DirectorySeparatorChar));

    return localPath;
}
```

## Release
+ See [CHANGELOG.md](CHANGELOG.md).

## Contributing
+ For more information about contributing PRs and issues, see [CONTRIBUTING.md](https://github.com/GovTechSG/csharp-apex-api-security/blob/master/.github/CONTRIBUTING.md).

## License
[MIT LICENSE ](https://github.com/GovTechSG/csharp-apex-api-security/blob/master/LICENSE)

## References
+ [Akana API Consumer Security](http://docs.akana.com/ag/cm_policies/using_api_consumer_app_sec_policy.htm)
+ [RSA and HMAC Request Signing Standard](http://tools.ietf.org/html/draft-cavage-http-signatures-05)

