﻿using ICanPay.Core;
using System.Threading;
using System.Threading.Tasks;

namespace ICanPay.Alipay
{
    /// <summary>
    /// 支付宝网关
    /// </summary>
    public sealed class AlipayGateway
        : GatewayBase, IFormPayment, IUrlPayment, IAppPayment, IScanPayment,IBarcodePayment
    {

        #region 私有字段

        private Merchant merchant;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化支付宝网关
        /// </summary>
        /// <param name="merchant">商户数据</param>
        public AlipayGateway(Merchant merchant)
            : base(merchant)
        {
            this.merchant = merchant;
        }

        #endregion

        #region 属性

        public override GatewayType GatewayType => GatewayType.Alipay;

        public override string GatewayUrl { get; set; } = "https://openapi.alipay.com/gateway.do";

        public new Merchant Merchant => merchant;

        public new Order Order => (Order)base.Order;

        public new Notify Notify => (Notify)base.Notify;

        protected override bool IsWaitPay => Notify.TradeStatus == "WAIT_BUYER_PAY";

        protected override bool IsSuccessPay => Notify.Code == "TRADE_SUCCESS";

        #endregion

        #region 方法

        public string BuildFormPayment()
        {
            InitFormPayment();

            return GatewayData.ToForm(GatewayUrl);
        }

        public string BuildUrlPayment()
        {
            InitUrlPayment();

            return $"{GatewayUrl}?{GetPaymentQueryString()}";
        }

        public string BuildAppPayment()
        {
            InitAppPayment();

            return GetPaymentQueryString();
        }

        public string BuildScanPayment()
        {
            PreCreate();

            return Notify.QrCode;
        }

        public void BuildBarcodePayment()
        {
            InitBarcodePayment();

            Commit(Constant.ALIPAY_TRADE_PAY_RESPONSE);

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(5000);
                Query();
                if (IsSuccessPay)
                {
                    OnPaymentSucceed(new PaymentSucceedEventArgs(this));
                    return;
                }
            }

            Cancel();
            OnPaymentFailed(new PaymentFailedEventArgs(this));
        }

        protected override async Task<bool> CheckNotifyDataAsync()
        {
            ReadNotify<Notify>();
            if (await IsSuccessResultAsync())
            {
                return true;
            }

            return false;
        }

        public void InitAppPayment()
        {
            Merchant.Method = Constant.APP;
            Order.ProductCode = Constant.QUICK_MSECURITY_PAY;

            InitOrderParameter();
        }

        public void InitFormPayment()
        {
            if (!string.IsNullOrEmpty(Merchant.ReturnUrl))
            {
                GatewayData.Add(Constant.RETURN_URL, Merchant.ReturnUrl);
            }

            Merchant.Method = Constant.WEB;
            Order.ProductCode = Constant.FAST_INSTANT_TRADE_PAY;

            InitOrderParameter();
        }

        public void InitUrlPayment()
        {
            if (!string.IsNullOrEmpty(Merchant.ReturnUrl))
            {
                GatewayData.Add(Constant.RETURN_URL, Merchant.ReturnUrl);
            }

            Merchant.Method = Constant.WAP;
            Order.ProductCode = Constant.QUICK_WAP_WAY;

            InitOrderParameter();
        }

        public void InitScanPayment()
        {
            if (!string.IsNullOrEmpty(Merchant.AppAuthToken))
            {
                GatewayData.Add(Constant.APP_AUTH_TOKEN, Merchant.AppAuthToken);
            }

            Merchant.Method = Constant.SCAN;

            InitOrderParameter();
        }

        public void InitBarcodePayment()
        {
            if (!string.IsNullOrEmpty(Merchant.AppAuthToken))
            {
                GatewayData.Add(Constant.APP_AUTH_TOKEN, Merchant.AppAuthToken);
            }

            Merchant.Method = Constant.BARCODE;
            Order.ProductCode = Constant.FACE_TO_FACE_PAYMENT;

            InitOrderParameter();
        }

        /// <summary>
        /// 初始化公共参数
        /// </summary>
        private void InitPublicParameter()
        {
            GatewayData.Add(Constant.APP_ID, Merchant.AppId);
            GatewayData.Add(Constant.METHOD, Merchant.Method);
            GatewayData.Add(Constant.FORMAT, Merchant.Format);
            GatewayData.Add(Constant.CHARSET, Merchant.Charset);
            GatewayData.Add(Constant.SIGN_TYPE, Merchant.SignType);
            GatewayData.Add(Constant.TIMESTAMP, Merchant.Timestamp.ToString(TIME_FORMAT));
            GatewayData.Add(Constant.VERSION, Merchant.Version);
            GatewayData.Add(Constant.NOTIFY_URL, Merchant.NotifyUrl);
            GatewayData.Add(Constant.BIZ_CONTENT, Merchant.BizContent);
        }

        /// <summary>
        /// 初始化订单参数
        /// </summary>
        private void InitOrderParameter()
        {
            Merchant.BizContent = Util.SerializeObject(Order);
            InitPublicParameter();
            Merchant.Sign = EncryptUtil.RSA2(GatewayData.ToUrl(), Merchant.Privatekey);
            GatewayData.Add(Constant.SIGN, Merchant.Sign);

            ValidateParameter(Merchant);
            ValidateParameter(Order);
        }

        public void InitQuery()
        {
            InitCommonParameter(Constant.QUERY);
        }

        public void InitCancel()
        {
            InitCommonParameter(Constant.CANCEL);
        }

        /// <summary>
        /// 初始化相应接口的参数
        /// </summary>
        /// <param name="method">接口名称</param>
        private void InitCommonParameter(string method)
        {
            Merchant.Method = method;
            Merchant.BizContent = $"{{\"out_trade_no\":\"{Order.OutTradeNo}\"}}";
            InitPublicParameter();
            Merchant.Sign = EncryptUtil.RSA2(GatewayData.ToUrl(), Merchant.Privatekey);
            GatewayData.Add(Constant.SIGN, Merchant.Sign);
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        private void Query()
        {
            InitQuery();

            Commit(Constant.ALIPAY_TRADE_QUERY_RESPONSE);
        }

        /// <summary>
        /// 撤销订单
        /// </summary>
        private void Cancel()
        {
            InitCancel();

            Commit(Constant.ALIPAY_TRADE_CANCEL_RESPONSE);
        }

        /// <summary>
        /// 提交请求
        /// </summary>
        /// <param name="type">结果类型</param>
        private void Commit(string type)
        {
            string result = HttpUtil
                .PostAsync(GatewayUrl, GatewayData.ToUrlEncode())
                .GetAwaiter()
                .GetResult();
            ReadReturnResult(result, type);
        }

        private string GetPaymentQueryString()
        {
            return GatewayData.ToUrlEncode();
        }

        /// <summary>
        /// 预创建订单
        /// </summary>
        /// <returns></returns>
        private void PreCreate()
        {
            InitOrderParameter();

            Commit(Constant.ALIPAY_TRADE_PRECREATE_RESPONSE);
        }

        /// <summary>
        /// 读取返回结果
        /// </summary>
        /// <param name="result">结果</param>
        /// <param name="key">结果的对象名</param>
        private void ReadReturnResult(string result, string key)
        {
            GatewayData.FromJson(result);
            string sign = GatewayData.GetStringValue(Constant.SIGN);
            result = GatewayData.GetStringValue(key);
            GatewayData.FromJson(result);
            ReadNotify<Notify>();
            Notify.Sign = sign;
        }

        /// <summary>
        /// 是否是已成功支付的支付通知
        /// </summary>
        /// <returns></returns>
        private async Task<bool> IsSuccessResultAsync()
        {
            if (ValidateNotifyParameter() && ValidateNotifySign() && await ValidateNotifyIdAsync())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查支付通知，是否支付成功，签名是否正确。
        /// </summary>
        /// <returns></returns>
        private bool ValidateNotifyParameter()
        {
            // 支付状态是否为成功。
            if (Notify.TradeStatus == Constant.TRADE_SUCCESS)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 验证支付宝通知的签名
        /// </summary>
        private bool ValidateNotifySign()
        {
            Merchant.Sign = EncryptUtil.RSA2(GatewayData.ToUrl(Constant.SIGN, Constant.SIGN_TYPE), Merchant.Privatekey);
            // 验证通知的签名
            if (Notify.Sign == Merchant.Sign)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 验证网关的通知Id是否有效
        /// </summary>
        private bool ValidateNotifyId()
        {
            string data = HttpUtil.ReadPage(GetValidateNotifyUrl());
            GatewayData.FromXml(data);
            // 服务器异步通知的通知Id则会在输出标志成功接收到通知的success字符串后失效。
            if (GatewayData.GetStringValue(Constant.IS_SUCCESS) == Constant.T)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 异步验证网关的通知Id是否有效
        /// </summary>
        private async Task<bool> ValidateNotifyIdAsync()
        {
            return await Task.Run(() => ValidateNotifyId());
        }

        /// <summary>
        /// 获得验证支付宝通知的Url
        /// </summary>
        private string GetValidateNotifyUrl()
        {
            return $"{GatewayUrl}?service=notify_verify&partner={Merchant.AppId}&notify_id={Notify.NotifyId}";
        }

        #endregion

    }
}