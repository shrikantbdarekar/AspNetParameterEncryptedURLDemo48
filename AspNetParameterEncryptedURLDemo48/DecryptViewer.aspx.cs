using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace AspNetParameterEncryptedURLDemo48
{
    public partial class DecryptViewer : SecurePage
    {
        protected override void OnPreLoad(EventArgs e)
        {
            base.OnPreLoad(e);

            if (IsPostBack)
                return;

            string isAjax = Request.QueryString["ajax"];

            if (!string.Equals(isAjax, "true", StringComparison.OrdinalIgnoreCase))
                return;

            var parameters = new SortedDictionary<string, string>();

            foreach (string key in Request.QueryString.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;

                if (string.Equals(key, "ajax", StringComparison.OrdinalIgnoreCase))
                    continue;

                parameters[key] = Request.QueryString[key];
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<table class='table table-striped table-bordered'>");
            sb.Append("<thead><tr><th>Parameter</th><th>Value</th></tr></thead>");
            sb.Append("<tbody>");

            foreach (var kvp in parameters)
            {
                sb.AppendFormat(
                    "<tr><td><strong>{0}</strong></td><td>{1}</td></tr>",
                    HttpUtility.HtmlEncode(kvp.Key),
                    HttpUtility.HtmlEncode(kvp.Value));
            }

            sb.Append("</tbody></table>");

            Response.Clear();
            Response.ContentType = "text/html";
            Response.Write(sb.ToString());
            Response.End();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var parameters = new SortedDictionary<string, string>();

                foreach (string key in Request.QueryString.AllKeys)
                {
                    if (string.IsNullOrEmpty(key))
                        continue;

                    parameters[key] = Request.QueryString[key];
                }

                if (parameters.Count == 0)
                {
                    pnlNoParams.Visible = true;
                    litTable.Text = string.Empty;
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("<table class='table table-striped table-bordered'>");
                sb.Append("<thead><tr><th>Parameter</th><th>Value</th></tr></thead>");
                sb.Append("<tbody>");

                foreach (var kvp in parameters)
                {
                    sb.AppendFormat(
                        "<tr><td><strong>{0}</strong></td><td>{1}</td></tr>",
                        HttpUtility.HtmlEncode(kvp.Key),
                        HttpUtility.HtmlEncode(kvp.Value));
                }

                sb.Append("</tbody></table>");

                litTable.Text = sb.ToString();
            }
        }
    }
}
