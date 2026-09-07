using System;
using System.Web;
using System.Web.UI;

namespace AspNetParameterEncryptedURLDemo48
{
    public partial class _Default : SecurePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // JavaScript URL — use SecureUrlHelper.Secure() for hidden field
                // JavaScriptUrl() adds quotes as JS string literal — not needed here
                hfJsUrl.Value = SecureUrlHelper.Secure(
                    "DecryptViewer.aspx?PatientID=12345&Name=John Smith&VisitDate=2026-09-01");

                // AJAX URL — use SecureUrlHelper.Secure()
                // Add &ajax=true so DecryptViewer can return raw HTML without master page
                hfAjaxUrl.Value = SecureUrlHelper.Secure(
                    "DecryptViewer.aspx?OrderID=9876&Customer=Jane Doe&Amount=250.00&ajax=true");

                // Literal HTML — use SecureUrlHelper.HtmlAttributeUrl()
                // Renders safe HTML href attribute
                litHtmlAnchor.Text =
                    "<a href=\"" +
                    SecureUrlHelper.HtmlAttributeUrl(
                        "DecryptViewer.aspx?UserID=555&Email=test@example.com&Role=Admin") +
                    "\" class=\"btn btn-warning\">" +
                    "User Profile via Literal HTML" +
                    "</a>";
            }
        }
    }
}
