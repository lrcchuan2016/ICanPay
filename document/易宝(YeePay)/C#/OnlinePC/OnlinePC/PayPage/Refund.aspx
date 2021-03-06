﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Refund.aspx.cs" Inherits="OnlinePC.PayPage.Refund" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=GBK"/>
     <title>网银退款功能</title>
    <link href="../css/temp.css" rel="stylesheet" type="text/css" />
</head>
<body>
     <form method="post" class="smart-green" runat="server">
        <h1 id="theme" runat="server">网银退款功能</h1>
        <label>
            <span>p0_Cmd:</span>
            <input id="p0_Cmd" type="text"  readonly="true" placeholder="业务类型" runat="server" />
        </label>

        <label>
            <span>p1_MerId</span>
            <input id="p1_MerId" type="text" placeholder="商户编号" readonly="true"  value="" runat="server"/>
        </label>

          <label>
            <span>p2_Order</span>
            <input id="p2_Order" type="text" placeholder="退款请求编号" value="" runat="server"/>
        </label>

        <label>
            <span>pb_TrxId</span>
            <input id="pb_TrxId" type="text" placeholder="易宝交易流水号" value="" runat="server"/>
        </label>

        <label>
            <span>p3_Amt</span>
            <input id="p3_Amt" type="text" placeholder="退款金额：单位 元" value="" runat="server" />
        </label>

         <label>
            <span>p4_Cur</span>
            <input id="p4_Cur" type="text" placeholder="交易币种" value="" runat="server" />
        </label>

         <label>
            <span>p5_Desc</span>
            <input id="p5_Desc" type="text" placeholder="退款说明" value="" runat="server" />
        </label>

        <label>
            <span>&nbsp;</span>
            <asp:Button ID="Button2" runat="server" class="button" Text="去退款" OnClick="Button1_Click" />
        </label>
    </form>
</body>
</html>






     
