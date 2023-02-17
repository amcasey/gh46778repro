## ASP.NET Core returns 500 if a client mangles TLS encrypted data

If a client application:

* creates a TLS connection to an ASP.NET core service
* somehow corrupts the encrypted data when before transmitting to the service

then the ASP.NET Core service will return a [500 Internal Server Error](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500) to the client instead of a [400 Bad Request](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400).

## The sample code

You can use the sample code in this repository to demonstrate the issue.  The sample code consists of a simple ASP.NET Core Web API application, along with a simple console application to call into it.  You can examine, build and/or run them via loading the `TlsBugRepro.sln` file into Visual Studio.

## Verifying sample apps work as expected

To demonstrate that the end to end code works when there's no corruption of the TLS encrypted data:

* In your command window, cd into the WebApplication1 directory and run `run.server.cmd`
* In another command window, cd into the ConsoleApp1 directory and run `run.client.with.clean.data.cmd`

With clean uncorrupted data, you will see the expected response from the service:

```

******************************************************************************************************************
Server response
******************************************************************************************************************
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: Fri, 17 Feb 2023 17:55:57 GMT
Server: Kestrel
Transfer-Encoding: chunked

431
{"message":"Hello from ConsoleApp1","details":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"}
0
```

## Reproducing the issue

To demonstrate that the ASP.NET Core web service returns a 500 HTTP status code when the client app somehow corrupts a single byte of the TLS encrypted data stream:

* In your command window, cd into the WebApplication1 directory and run `run.server.cmd`
* In another command window, cd into the ConsoleApp1 directory and run `run.client.with.corrupted.data.cmd`

With a single byte of corrupted TLS data, you will see that the service returns a 500 HTTP status code:

```
******************************************************************************************************************
Server response
******************************************************************************************************************
HTTP/1.1 500 Internal Server Error
Content-Type: text/plain; charset=utf-8
Date: Fri, 17 Feb 2023 18:02:37 GMT
Server: Kestrel
Transfer-Encoding: chunked

cec
System.IO.IOException: The decryption operation failed, see inner exception.
 ---> System.ComponentModel.Win32Exception (0x80090330): The specified data could not be decrypted.
   --- End of inner exception stack trace ---
   at System.Net.Security.SslStream.ReadAsyncInternal[TIOAdapter](TIOAdapter adapter, Memory`1 buffer)
   at System.IO.Pipelines.StreamPipeReader.<ReadAsync>g__Core|36_0(StreamPipeReader reader, CancellationTokenSource tokenSource, CancellationToken cancellationToken)
   at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ContentLengthMessageBody.ReadAsyncInternal(CancellationToken cancellationToken)
   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
   at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpRequestStream.ReadAsyncInternal(Memory`1 destination, CancellationToken cancellationToken)
   at System.Text.Json.JsonSerializer.ReadFromStreamAsync(Stream utf8Json, ReadBufferState bufferState, CancellationToken cancellationToken)
   at System.Text.Json.JsonSerializer.ReadAllAsync[TValue](Stream utf8Json, JsonTypeInfo jsonTypeInfo, CancellationToken cancellationToken)
   at Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter.ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
   at Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter.ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
   at Microsoft.AspNetCore.Mvc.ModelBinding.Binders.BodyModelBinder.BindModelAsync(ModelBindingContext bindingContext)
   at Microsoft.AspNetCore.Mvc.ModelBinding.ParameterBinder.BindModelAsync(ActionContext actionContext, IModelBinder modelBinder, IValueProvider valueProvider, ParameterDescriptor parameter, ModelMetadata metadata, Object value, Object container)
   at Microsoft.AspNetCore.Mvc.Controllers.ControllerBinderDelegateProvider.<>c__DisplayClass0_0.<<CreateBinderDelegate>g__Bind|0>d.MoveNext()
--- End of stack trace from previous location ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|6_0(Endpoint endpoint, Task requestTask, ILogger logger)
   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
   at Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware.InvokeInternal(HttpContext context)
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware.Invoke(HttpContext context)

HEADERS
=======
Host: 127.0.0.1:7000
Content-Type: application/json; charset=utf-8
Content-Length: 1073

0
```


