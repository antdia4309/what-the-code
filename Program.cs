using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WhatTheCode.Components;
using WhatTheCode.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<ProgressStore>();
builder.Services.AddScoped<BadgeStore>();
builder.Services.AddScoped<DraftLevel>();

await builder.Build().RunAsync();
