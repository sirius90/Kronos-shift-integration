#pragma checksum "C:\githubclone\Kronos-Shifts-Connector\Microsoft.Teams.Shifts.Integration\Microsoft.Teams.Shifts.Integration.Configuration\Views\Shared\_StatusMessages.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "22be0fc1ee93a98b47f7d6401f43a58e923a6720"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Shared__StatusMessages), @"mvc.1.0.view", @"/Views/Shared/_StatusMessages.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Shared/_StatusMessages.cshtml", typeof(AspNetCore.Views_Shared__StatusMessages))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "C:\githubclone\Kronos-Shifts-Connector\Microsoft.Teams.Shifts.Integration\Microsoft.Teams.Shifts.Integration.Configuration\Views\_ViewImports.cshtml"
using Microsoft.Teams.Shifts.Integration.Configuration;

#line default
#line hidden
#line 2 "C:\githubclone\Kronos-Shifts-Connector\Microsoft.Teams.Shifts.Integration\Microsoft.Teams.Shifts.Integration.Configuration\Views\_ViewImports.cshtml"
using Microsoft.Teams.Shifts.Integration.Configuration.Models;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"22be0fc1ee93a98b47f7d6401f43a58e923a6720", @"/Views/Shared/_StatusMessages.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0335bca9cdfd2227969ab224957063cabe8bda96", @"/Views/_ViewImports.cshtml")]
    public class Views_Shared__StatusMessages : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 1 "C:\githubclone\Kronos-Shifts-Connector\Microsoft.Teams.Shifts.Integration\Microsoft.Teams.Shifts.Integration.Configuration\Views\Shared\_StatusMessages.cshtml"
  
    var type = (string)TempData["_alert.type"];
    var title = (string)TempData["_alert.title"];
    var body = (string)TempData["_alert.body"];

#line default
#line hidden
#line 6 "C:\githubclone\Kronos-Shifts-Connector\Microsoft.Teams.Shifts.Integration\Microsoft.Teams.Shifts.Integration.Configuration\Views\Shared\_StatusMessages.cshtml"
 if (!string.IsNullOrEmpty(type))
{

#line default
#line hidden
            BeginContext(194, 8, true);
            WriteLiteral("    <div");
            EndContext();
            BeginWriteAttribute("class", " class=\"", 202, "\"", 245, 4);
            WriteAttributeValue("", 210, "alert", 210, 5, true);
            WriteAttributeValue(" ", 215, "alert-", 216, 7, true);
#line 8 "C:\githubclone\Kronos-Shifts-Connector\Microsoft.Teams.Shifts.Integration\Microsoft.Teams.Shifts.Integration.Configuration\Views\Shared\_StatusMessages.cshtml"
WriteAttributeValue("", 222, type, 222, 5, false);

#line default
#line hidden
            WriteAttributeValue(" ", 227, "alert-dismissible", 228, 18, true);
            EndWriteAttribute();
            BeginContext(246, 166, true);
            WriteLiteral(" role=\"alert\">\r\n        <button type=\"button\" class=\"close\" data-dismiss=\"alert\" aria-label=\"Close\"><span aria-hidden=\"true\">&times;</span></button>\r\n        <strong>");
            EndContext();
            BeginContext(413, 5, false);
#line 10 "C:\githubclone\Kronos-Shifts-Connector\Microsoft.Teams.Shifts.Integration\Microsoft.Teams.Shifts.Integration.Configuration\Views\Shared\_StatusMessages.cshtml"
           Write(title);

#line default
#line hidden
            EndContext();
            BeginContext(418, 10, true);
            WriteLiteral("</strong> ");
            EndContext();
            BeginContext(429, 4, false);
#line 10 "C:\githubclone\Kronos-Shifts-Connector\Microsoft.Teams.Shifts.Integration\Microsoft.Teams.Shifts.Integration.Configuration\Views\Shared\_StatusMessages.cshtml"
                           Write(body);

#line default
#line hidden
            EndContext();
            BeginContext(433, 14, true);
            WriteLiteral("\r\n    </div>\r\n");
            EndContext();
#line 12 "C:\githubclone\Kronos-Shifts-Connector\Microsoft.Teams.Shifts.Integration\Microsoft.Teams.Shifts.Integration.Configuration\Views\Shared\_StatusMessages.cshtml"
}

#line default
#line hidden
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591
