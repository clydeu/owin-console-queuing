using Microsoft.Owin;
using System;
using System.IO;
using System.Threading;
using MidFunc = System.Func<
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
       >;

namespace owin_console_queuing.Middlewares
{
    public class MaxRequestsMiddleware
    {
        private int queuedRequestCounter;
        private SemaphoreSlim blocker;
        private readonly int maxAcceptedRequests;

        public MaxRequestsMiddleware(int maxAcceptedRequests, int maxConcurrentRequests)
        {
            queuedRequestCounter = 0;
            blocker = new SemaphoreSlim(0, maxConcurrentRequests);
            blocker.Release(maxConcurrentRequests);
            if (maxAcceptedRequests <= 0)
            {
                maxAcceptedRequests = int.MaxValue;
            }

            this.maxAcceptedRequests = maxAcceptedRequests;

            Console.WriteLine($"{nameof(MaxRequestsMiddleware)} configured with {nameof(maxAcceptedRequests)}={maxAcceptedRequests} and {nameof(maxConcurrentRequests)}={maxConcurrentRequests}.");
        }

        internal MidFunc MaxAcceptedRequests()
            => next =>
               async env =>
                {
                    try
                    {
                        int requests = Interlocked.Increment(ref queuedRequestCounter);
                        if (requests > maxAcceptedRequests)
                        {
                            var body = new StreamReader((Stream)env["owin.RequestBody"]).ReadToEnd();
                            Console.WriteLine("Accepted requests limit ({0}) reached. Request [{1}] {2}{3} rejected. {4}", maxAcceptedRequests, env["owin.RequestMethod"], env["owin.RequestPath"], env["owin.RequestQueryString"], body);
                            var response = new OwinContext(env).Response;
                            response.StatusCode = 503;
                            response.ReasonPhrase = "Service Unavailable";
                            response.Write(response.ReasonPhrase);
                            return;
                        }
                        await next(env);
                    }
                    finally
                    {
                        int concurrentRequests = Interlocked.Decrement(ref queuedRequestCounter);
                    }
                };

        internal MidFunc MaxConcurrentRequests()
            => next =>
               async env =>
               {
                   blocker.Wait();
                   try
                   {
                       await next(env);
                   }
                   finally
                   {
                       blocker.Release();
                   }
               };
    }
}