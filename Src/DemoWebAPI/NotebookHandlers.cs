using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Notebook;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoWebAPI
{
    public class NotebookHandlers
    {
        private IApplicationBuilder app;
        private RouteBuilder routeBuilder;
        private Dictionary<string, NotebookHandler> handlers = new Dictionary<string, NotebookHandler>();

        public NotebookHandlers(IApplicationBuilder app)
        {
            this.app = app;
            this.routeBuilder = new RouteBuilder(app);

            app.Use(async (ctx, next) =>
            {
                var path = ctx.Request.Path.ToString().Substring(1);
                if (path == "notebook")
                {
                    var q = ctx.Request.Query;
                    var notebookName = q["name"].ToString();

                    var notebookBytes = new byte[ctx.Request.ContentLength ?? 0];
                    var read = ctx.Request.Body.ReadAsync(notebookBytes, 0, notebookBytes.Length);
                    var text = Encoding.UTF8.GetString(notebookBytes);

                    var handler = new NotebookHandler(notebookName, notebookBytes);

                    if (handlers.ContainsKey(notebookName)) handlers.Remove(notebookName);
                    handlers.Add(notebookName, handler);
                }
                else if (handlers.ContainsKey(path))
                {
                    var handler = handlers[path];
                    await handler.Handle(ctx);
                }
            });

            app.UseRouter(routeBuilder.Build());

        }

    }
}