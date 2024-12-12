using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using App.Model;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;
using App.Model;
using System.Linq;
using System.Text;
using AppMvc.Net.Model;

namespace App.Controllers
{
    public class PaymentController : Controller
    {
        private readonly HttpClient client = null;
        private string api;
        private static Random random = new Random();
        public PaymentController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            this.api = "https://localhost:44379/api/payment";
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Cancel()
        {
            return View();
        }
        public static string GenerateRandomOrder()
        {
            const string digits = "0123456789";
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            StringBuilder stringBuilder = new StringBuilder(9);

            // Sinh 6 chữ số ngẫu nhiên
            for (int i = 0; i < 6; i++)
            {
                stringBuilder.Append(digits[random.Next(digits.Length)]);
            }

            // Sinh 3 chữ cái in hoa ngẫu nhiên
            for (int i = 0; i < 3; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }

            // Trộn ngẫu nhiên chuỗi kết quả
            string result = stringBuilder.ToString();
            result = new string(result.ToCharArray().OrderBy(x => random.Next()).ToArray());

            return result;
        }
        [HttpGet]
        public async Task<IActionResult> CheckOut(Order order)
        {
            if (order.PaymentMethodId != 1)
            {
                Payment obj = new Payment();
                obj.PaymentContent = "Thanh toan don hang " + order.IdOrder + GenerateRandomOrder();
                obj.PaymentCurrency = "VND";
                obj.PaymentRefId = "" + order.IdOrder;
                if (order.OrderTotalDiscount > 0)
                {
                    obj.RequiredAmount = (decimal)(order.OrderTotalDiscount);
                }
                else
                {
                    obj.RequiredAmount = (decimal)(order.OrderTotal);
                }

                obj.PaymentDate = DateTime.UtcNow;
                obj.PaymentLanguage = "en";
                obj.MerchantId = "MER0001";
                if (order.PaymentMethodId == 2)
                    obj.PaymentDestinationId = "VNPAY";
                else
                    obj.PaymentDestinationId = "MOMO";
                obj.Signature = "" + GenerateRandomOrder();
                RePayment rePay = new RePayment(obj.PaymentContent, obj.PaymentCurrency, obj.PaymentRefId, obj.RequiredAmount, obj.MerchantId,
                obj.PaymentLanguage, obj.PaymentDestinationId, obj.Signature);
                string data = JsonSerializer.Serialize(obj);
                var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(api, content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    // Phân tích chuỗi JSON thành một đối tượng JObject
                    JObject responseData = JObject.Parse(jsonResponse);
                    // Truy cập thuộc tính "data" trong đối tượng JSON
                    JObject dataRes = (JObject)responseData["data"];
                    // Truy cập thuộc tính "paymentUrl"
                    string paymentUrl = (string)dataRes["paymentUrl"];
                    ViewBag.UrlLink = paymentUrl;
                    return Redirect(paymentUrl);
                }
            }
            return View("Index");
        }
        [HttpGet]
        public async Task<IActionResult> ReCheckOut(Order order)
        {
            if (order.PaymentMethodId != 1)
            {
                RePayment payment = new RePayment();
                string data = JsonSerializer.Serialize(payment);
                var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(api, content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    // Phân tích chuỗi JSON thành một đối tượng JObject
                    JObject responseData = JObject.Parse(jsonResponse);
                    // Truy cập thuộc tính "data" trong đối tượng JSON
                    JObject dataRes = (JObject)responseData["data"];
                    // Truy cập thuộc tính "paymentUrl"
                    string paymentUrl = (string)dataRes["paymentUrl"];
                    ViewBag.UrlLink = paymentUrl;
                    return Redirect(paymentUrl);
                }
            }
            return View("Index");
        }
    }
}
