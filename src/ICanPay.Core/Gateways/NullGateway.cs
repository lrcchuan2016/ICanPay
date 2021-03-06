﻿using System.Threading.Tasks;

namespace ICanPay.Core
{
    /// <summary>
    /// 未知网关
    /// </summary>
    public class NullGateway : GatewayBase
    {

        #region 构造函数

        /// <summary>
        /// 初始化未知网关
        /// </summary>
        public NullGateway()
        {
        }

        /// <summary>
        /// 初始化未知网关
        /// </summary>
        /// <param name="gatewayData">网关数据</param>
        public NullGateway(GatewayData gatewayData)
            : base(gatewayData)
        {
        }

        #endregion

        #region 属性

        public override GatewayType GatewayType => GatewayType.None;

        public override string GatewayUrl { get; set; } = string.Empty;

        protected override bool IsSuccessPay => false;

        protected override bool IsWaitPay => false;

        #endregion

        #region 方法

        protected override async Task<bool> CheckNotifyDataAsync()
        {
            return await Task.Run(() => { return false; });
        }

        #endregion

    }
}
