namespace ConsoleApp1
{
    public class TracingHandler : DelegatingHandler
    {
        public TracingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Tracer.Info("Request:");
            Tracer.Verbose(request.ToString());
            if (request.Content != null)
            {
                Tracer.Verbose(await request.Content.ReadAsStringAsync());
            }
            Tracer.Verbose("");

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Tracer.Verbose("Response:");
            Tracer.Verbose(response.ToString());
            if (response.Content != null)
            {
                Tracer.Verbose(await response.Content.ReadAsStringAsync());
            }
            Tracer.Verbose("");

            return response;
        }
    }
}
