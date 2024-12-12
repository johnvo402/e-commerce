using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppMvc.Net.Model
{
    public class RePayment
    {
        public string? PaymentContent { get; set; }
        public string? PaymentCurrency { get; set; }
        public string? PaymentRefId { get; set; }
        public decimal RequiredAmount { get; set; }
        public DateTime? PaymentDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpireDate { get; set; } = DateTime.UtcNow;
        public string? PaymentLanguage { get; set; }
        public string? MerchantId { get; set; }
        public string? PaymentDestinationId { get; set; }
        public string? Signature { get; set; }
        public decimal? RequiredAmount1 { get; }

        public RePayment(string paymentContent, string paymentCurrency, string paymentRefId, decimal requiredAmount,
                        string paymentLanguage, string merchantId,
                        string paymentDestinationId, string signature)
        {
            PaymentContent = paymentContent;
            PaymentCurrency = paymentCurrency;
            PaymentRefId = paymentRefId;
            RequiredAmount = requiredAmount;
            PaymentLanguage = paymentLanguage;
            MerchantId = merchantId;
            PaymentDestinationId = paymentDestinationId;
            Signature = signature;
        }
        public RePayment(){}
    }
}