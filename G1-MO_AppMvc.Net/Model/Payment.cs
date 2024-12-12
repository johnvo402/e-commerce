using System;

namespace App.Model
{
    public class Payment
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
    }
}
