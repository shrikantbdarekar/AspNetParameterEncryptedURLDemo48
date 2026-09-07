<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AspNetParameterEncryptedURLDemo48._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main>
        <section class="row" aria-labelledby="aspnetTitle">
            <h1 id="aspnetTitle">ASP.NET</h1>
            <p class="lead">ASP.NET is a free web framework for building great Web sites and Web applications using HTML, CSS, and JavaScript.</p>
            <p><a href="http://www.asp.net" class="btn btn-primary btn-md">Learn more &raquo;</a></p>
        </section>

        <div class="row">
            <section class="col-md-12">
                <h2>Encrypted URL Demo Links</h2>
                <p class="lead">Click any link below to test encrypted URL parameters. SecureControlProcessor auto-encrypts HyperLink NavigateUrl on render. SecurePage decrypts the token on the next request.</p>

                <ul class="list-group">
                    <li class="list-group-item">
                        <asp:HyperLink ID="hlPatient" runat="server"
                            NavigateUrl="DecryptViewer.aspx?PatientID=12345&Name=John Smith&VisitDate=2026-09-01"
                            Text="Patient Record — PatientID=12345, Name=John Smith, VisitDate=2026-09-01" />
                    </li>
                    <li class="list-group-item">
                        <asp:HyperLink ID="hlOrder" runat="server"
                            NavigateUrl="DecryptViewer.aspx?OrderID=9876&Customer=Jane Doe&Amount=250.00"
                            Text="Order Details — OrderID=9876, Customer=Jane Doe, Amount=250.00" />
                    </li>
                    <li class="list-group-item">
                        <asp:HyperLink ID="hlUser" runat="server"
                            NavigateUrl="DecryptViewer.aspx?UserID=555&Email=test@example.com&Role=Admin"
                            Text="User Profile — UserID=555, Email=test@example.com, Role=Admin" />
                    </li>
                    <li class="list-group-item">
                        <asp:HyperLink ID="hlDoc" runat="server"
                            NavigateUrl="DecryptViewer.aspx?DocID=42&Type=Invoice&Confidential=true"
                            Text="Document Access — DocID=42, Type=Invoice, Confidential=true" />
                    </li>
                    <li class="list-group-item">
                        <asp:HyperLink ID="hlProduct" runat="server"
                            NavigateUrl="DecryptViewer.aspx?ProductCode=SKU-998877&Category=Electronics&InStock=yes"
                            Text="Product Lookup — ProductCode=SKU-998877, Category=Electronics, InStock=yes" />
                    </li>
                    <li class="list-group-item">
                        <asp:HyperLink ID="hlEmployee" runat="server"
                            NavigateUrl="DecryptViewer.aspx?EmpID=E10234&Department=Engineering&Location=Building A"
                            Text="Employee Record — EmpID=E10234, Department=Engineering, Location=Building A" />
                    </li>
                    <li class="list-group-item">
                        <asp:HyperLink ID="hlTicket" runat="server"
                            NavigateUrl="DecryptViewer.aspx?TicketID=TKT-5678&Priority=High&Status=Open"
                            Text="Support Ticket — TicketID=TKT-5678, Priority=High, Status=Open" />
                    </li>
                    <li class="list-group-item">
                        <asp:HyperLink ID="hlPayment" runat="server"
                            NavigateUrl="DecryptViewer.aspx?TransID=TXN-4455&Amount=1299.99&Currency=USD&Method=CreditCard"
                            Text="Payment Transaction — TransID=TXN-4455, Amount=1299.99, Currency=USD, Method=CreditCard" />
                    </li>
                    <li class="list-group-item">
                        <asp:HyperLink ID="hlShipment" runat="server"
                            NavigateUrl="DecryptViewer.aspx?TrackingNo=SHP-11223&Carrier=FedEx&ETA=2026-09-15"
                            Text="Shipment Tracking — TrackingNo=SHP-11223, Carrier=FedEx, ETA=2026-09-15" />
                    </li>
                    <li class="list-group-item">
                        <asp:HyperLink ID="hlMulti" runat="server"
                            NavigateUrl="DecryptViewer.aspx?A=Alpha&B=Bravo&C=Charlie&D=Delta"
                            Text="Multi-Param Test — A=Alpha, B=Bravo, C=Charlie, D=Delta" />
                    </li>
                </ul>
            </section>
        </div>

        <div class="row" style="margin-top:20px;">
            <section class="col-md-4">
                <h3>JavaScript URL</h3>
                <p>Encrypted URL passed to <code>window.location.href</code> via <code>SecureUrlHelper.JavaScriptUrl()</code>.</p>
                <asp:HiddenField ID="hfJsUrl" runat="server" />
                <button type="button" class="btn btn-info" onclick="var u=document.getElementById('<%= hfJsUrl.ClientID %>').value;window.location.href=u;">
                    Navigate via window.location
                </button>
            </section>

            <section class="col-md-4">
                <h3>AJAX URL</h3>
                <p>Encrypted URL used in <code>fetch()</code> call via <code>SecureUrlHelper.Secure()</code>.</p>
                <asp:HiddenField ID="hfAjaxUrl" runat="server" />
                <button type="button" class="btn btn-success" onclick="var u=document.getElementById('<%= hfAjaxUrl.ClientID %>').value;fetch(u).then(function(r){return r.text()}).then(function(t){document.getElementById('ajaxResult').innerText=t}).catch(function(e){document.getElementById('ajaxResult').innerText='Error: '+e})">
                    Fetch via AJAX
                </button>
                <pre id="ajaxResult" style="margin-top:10px;max-height:200px;overflow:auto;"></pre>
            </section>

            <section class="col-md-4">
                <h3>Literal HTML &lt;a href&gt;</h3>
                <p>Plain HTML anchor rendered via <code>&lt;asp:Literal&gt;</code> using <code>SecureUrlHelper.HtmlAttributeUrl()</code>.</p>
                <asp:Literal ID="litHtmlAnchor" runat="server" />
            </section>
        </div>
    </main>

</asp:Content>
