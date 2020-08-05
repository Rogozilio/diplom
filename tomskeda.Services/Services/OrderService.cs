﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tomskeda.Core.Entities;
using Tomskeda.Services.Interfaces;
using Yandex.Checkout.V3;

namespace Tomskeda.Services.Services
{
    public class OrderService : IOrderService
    {
        /// <summary>
        /// Подготовка к отправка заказа
        /// </summary>
        /// <param name="cookie">Данные куки</param>
        public async void SendOrder(Order order, Core.Entities.Cookie cookie, int price)
        {
            string data = null;
            string[] product = cookie.Ids.Split(',');
            string[] product_kol = cookie.Counts.Split(',');
            string[] product_price = new string[product.Length];

            Dictionary<string, string> param = new Dictionary<string, string>(11);
            param["secret"] = "2bhDGziZHfR44EtHKSRGi2Dh69daTShGTt9nShanrRdfhARTdtnreSBaREbAhk65F6ekbeBtZED64fQ7B4kGHQHkfKKrZFhsArEDz5ZfzkantQiYFAsba3rfA4AiyGDYZ6kf28niirBE6KkB8FD9tSeTGE559yQZr36b2fn8AQDtnQTkhiiD4shAYbsYz6GNTBRGH6E7H6r2H89Sfih9DsAkyR7SHDts492A5syBkNkYHkDSzKS35danT7";
            param["name"] = order.Name;
            param["mail"] = order.Email;
            param["phone"] = order.Phone;
            param["street"] = order.Street;
            param["home"] = order.House;
            param["pod"] = order.Porch;
            param["et"] = order.Floor;
            param["apart"] = order.Apartment;
            param["descr"] = order.Comment;
            param["datetime"] = new DateService().GetDateFrontpad(int.Parse(cookie.Day)) + order.Date;
            param["pay"] = order.Pay;

            foreach (KeyValuePair<string, string> item in param)
            {
                data += "&" + item.Key + '=' + item.Value;
            }

            SortProduct(ref product, ref product_kol, ref product_price);

            for (int i = 0; i < product.Length; i++)
            {
                data += "&product[" + i + "]=" + product[i] + "";
                data += "&product_kol[" + i + "]=" + product_kol[i] + "";
                if (i < product_price.Length && product_price.Length != 3)
                {
                    data += "&product_price[" + i + "]=" + product_price[i] + "";
                }

                if (i == product.Length - 1 && price < 300)
                {
                    data += "&product[" + (i + 1) + "]=" + 1 + "";
                    data += "&product_kol[" + (i + 1) + "]=" + 1 + "";
                }
            }
            await SendInFrontPad("https://app.frontpad.ru/api/index.php?new_order", data);
        }
        /// <summary>
        /// Отправка заказа в интерфейс frontpad
        /// </summary>
        /// <param name="typeURL">ссылка куда отправить</param>
        /// <param name="data">данные отправления</param>
        /// <returns></returns>
        private async Task<string> SendInFrontPad(string typeURL, string data)
        {
            string answer = "";
            WebRequest request = WebRequest.Create(typeURL);
            request.Method = "POST";
            // преобразуем данные в массив байтов
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(data);
            // устанавливаем тип содержимого - параметр ContentType
            request.ContentType = "application/x-www-form-urlencoded";
            // Устанавливаем заголовок Content-Length запроса - свойство ContentLength
            request.ContentLength = byteArray.Length;
            //записываем данные в поток запроса
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            WebResponse response = await request.GetResponseAsync();
            response.ToString();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    answer = reader.ReadToEnd();
                }
            }
            response.Close();
            return answer;
        }
        /// <summary>
        /// Возвращает ссылку для осуществления оплаты
        /// </summary>
        /// <param name="price">цена заказа</param>
        /// <returns></returns>
        public string PaymentYandex(Order order, int price)
        {
            var client = new Client(
            shopId: "635308",
            secretKey: "live_Wz46H8myyA8WeMckb1wSAksHU22H-sFQqDWmcN9ULJk");

            var newPayment = new NewPayment
            {
                Amount = new Amount { Value = price, Currency = "RUB" },
                Confirmation = new Confirmation
                {
                    Type = ConfirmationType.Redirect,
                    ReturnUrl = "https://tomskeda.ru/Order/OrderSendr"
                },
                PaymentMethodData = new PaymentMethod { Type = "bank_card" },
                Description = order.Name + ", полсе успешной оплаты ожидайте СМС на номер " + order.Phone + "."
            };
            Payment payment = client.CreatePayment(newPayment);
            return payment.Confirmation.ConfirmationUrl;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="product_kol"></param>
        /// <param name="product_price"></param>
        private void SortProduct(ref string[] product, ref string[] product_kol, ref string[] product_price)
        {
            List<string> productPrice = new List<string>() { "0" };
            List<string> productBase = new List<string>();
            List<string> productKomplex = new List<string>() { "2" };
            List<string> kolBase = new List<string>();
            List<string> kolKomplex = new List<string>();
            int komplexi = 0;
            string kompot = "";
            int j = 0;
            for (int i = 0; i < product.Length; i++)
            {
                if (product[i] != "2")
                {
                    productBase.Add(product[i]);
                    kolBase.Add(product_kol[j++]);
                }
                else
                {
                    i++;
                    komplexi += int.Parse(product_kol[j]);
                    for (; product[i] != "259" && product[i] != "254"; i++)
                    {
                        productPrice.Add("0");
                        productKomplex.Add(product[i]);
                        kolKomplex.Add(product_kol[j]);
                    }
                    j++;
                    kompot = product[i];
                    i = i + 2;
                }
            }
            kolKomplex.Insert(0, komplexi.ToString());
            productPrice.AddRange(new string[] { "0", "0", "0" });
            productKomplex.AddRange(new string[] { kompot, "275", "348" });
            kolKomplex.AddRange(new string[] { komplexi.ToString(), komplexi.ToString(), komplexi.ToString() });
            productKomplex.AddRange(productBase);
            kolKomplex.AddRange(kolBase);
            product = productKomplex.ToArray();
            product_kol = kolKomplex.ToArray();
            product_price = productPrice.ToArray();
        }
    }
}