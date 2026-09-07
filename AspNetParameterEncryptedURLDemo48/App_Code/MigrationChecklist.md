# Migration Checklist

## Step 1 – Install core classes

Ensure these classes are present:

- UrlSecurity.cs
- SecureUrl.cs
- SecureControlProcessor.cs
- SecurePage.cs
- SecureUrlHelper.cs

## Step 2 – Configure keys

Add the URL encryption and validation keys to web.config.

Never commit production secrets to a public repository.

## Step 3 – Enable SecurePage

Where appropriate, have pages inherit from SecurePage, or integrate its processing into your existing BasePage.

## Step 4 – Automatic controls

Existing server controls such as HyperLink/HtmlAnchor can remain unchanged.

## Step 5 – Review special cases

Search the solution for:

- Response.Redirect
- JavaScript URL creation
- AJAX URL creation
- raw href
- literal HTML
- startup scripts
- PageMethods/WebMethods
- dynamically generated HTML

## Step 6 – Replace only the URL construction

Do not rewrite business logic.

Example:

Before:

```csharp
Response.Redirect("Patient.aspx?PatientID=" + id);
```

After:

```csharp
Response.Redirect(
    SecureUrlHelper.Secure(
        "Patient.aspx?PatientID=" + id));
```

## Step 7 – Test

Test:

- normal link
- GridView
- Repeater
- redirect
- JavaScript navigation
- AJAX GET
- invalid token
- modified token
- missing token
- multiple query parameters
- special characters
- URL without query string
- external URL
- unauthorized patient/record

## Step 8 – Security review

Confirm:

- HTTPS is enforced
- encryption keys are protected
- authorization is performed after decryption
- invalid tokens return an appropriate error
- sensitive token data is not logged
- production keys are different from development keys
