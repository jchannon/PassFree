using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using PassFree.Components;

namespace PassFree.TagHelpers;

[HtmlTargetElement("passfree")]
public class PassFreeLoginTagHelper : TagHelper
{
    private readonly IOptions<PassFreeOptions> _options;
    private readonly HtmlRenderer _htmlRenderer;

    public PassFreeLoginTagHelper(IOptions<PassFreeOptions> options, HtmlRenderer htmlRenderer)
    {
        _options = options;
        _htmlRenderer = htmlRenderer;
    }

    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    [HtmlAttributeName("login-path")]
    public string? LoginPath { get; set; }

    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null; // Don't render a wrapper element

        // Create parameters for the Razor Component
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
        });

        // Render the Razor Component to HTML
        var html = await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await _htmlRenderer.RenderComponentAsync<PassFreeLogin>(parameters);
            return result.ToHtmlString();
        });

        output.Content.SetHtmlContent(html);
    }
}
