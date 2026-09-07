using System;
using System.Web;
using System.Web.UI;

/// <summary>
/// Examples only. Copy the relevant snippets into your application.
/// </summary>
public class UsageExamples
{
    // ------------------------------------------------------------
    // 1. Response.Redirect()
    // ------------------------------------------------------------

    public void RedirectExample(Page page, int patientId, int visitId)
    {
        string url = SecureUrlHelper.Secure(
            "Patient.aspx",
            "PatientID", patientId.ToString(),
            "VisitID", visitId.ToString());

        page.Response.Redirect(url, false);
        HttpContext.Current.ApplicationInstance.CompleteRequest();
    }


    // ------------------------------------------------------------
    // 2. Server-side URL for JavaScript
    // ------------------------------------------------------------

    public void JavaScriptLocationExample(
        Page page,
        int patientId)
    {
        string url =
            SecureUrlHelper.JavaScriptUrl(
                "Patient.aspx?PatientID=" +
                patientId);

        string script =
            "window.location.href=" +
            url +
            ";";

        page.ClientScript.RegisterStartupScript(
            page.GetType(),
            "OpenPatient",
            script,
            true);
    }


    // ------------------------------------------------------------
    // 3. window.open()
    // ------------------------------------------------------------

    public void WindowOpenExample(
        Page page,
        int patientId)
    {
        string url =
            SecureUrlHelper.JavaScriptUrl(
                "Patient.aspx?PatientID=" +
                patientId);

        string script =
            "window.open(" +
            url +
            ", '_blank');";

        page.ClientScript.RegisterStartupScript(
            page.GetType(),
            "OpenPatientWindow",
            script,
            true);
    }


    // ------------------------------------------------------------
    // 4. AJAX URL
    // ------------------------------------------------------------
    //
    // Do NOT put the encryption key in JavaScript.
    //
    // Generate the secure URL on the server and return it
    // through your AJAX response/configuration.
    //

    public string GetPatientAjaxUrl(int patientId)
    {
        return SecureUrlHelper.Secure(
            "PatientData.aspx?PatientID=" +
            patientId);
    }


    // ------------------------------------------------------------
    // 5. Raw <a href=""> HTML
    // ------------------------------------------------------------

    public string CreateRawAnchor(int patientId)
    {
        string url =
            SecureUrlHelper.HtmlAttributeUrl(
                "Patient.aspx?PatientID=" +
                patientId);

        return "<a href=\"" +
               url +
               "\">Open Patient</a>";
    }


    // ------------------------------------------------------------
    // 6. HTML generated as literal string
    // ------------------------------------------------------------

    public string CreateButtonHtml(int patientId)
    {
        string url =
            SecureUrlHelper.HtmlAttributeUrl(
                "Patient.aspx?PatientID=" +
                patientId);

        return
            "<button type=\"button\" " +
            "data-url=\"" + url + "\" " +
            "onclick=\"location.href=this.dataset.url\">" +
            "Open Patient" +
            "</button>";
    }


    // ------------------------------------------------------------
    // 7. GridView / Repeater when URL is manually constructed
    // ------------------------------------------------------------

    public string CreateGridUrl(object patientId)
    {
        return SecureUrlHelper.Secure(
            "Patient.aspx?PatientID=" +
            Convert.ToString(patientId));
    }


    // ------------------------------------------------------------
    // 8. Response.Redirect() with existing string
    // ------------------------------------------------------------

    public void ExistingRedirect(Page page, string patientId)
    {
        string url =
            "Patient.aspx?PatientID=" +
            HttpUtility.UrlEncode(patientId);

        page.Response.Redirect(
            SecureUrlHelper.Secure(url),
            false);

        HttpContext.Current.ApplicationInstance.CompleteRequest();
    }
}
