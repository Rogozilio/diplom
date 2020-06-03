﻿using bdtest.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace bdtest.Functions
{
    public class Order
    {
        /// <summary>
        /// Куки ИД продуктов
        /// </summary>
        public string CookieId;
        /// <summary>
        /// Куки количества на еденицу продукта
        /// </summary>
        public string CookieCount;
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Электронная почта
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Сотовый телефон
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Улица
        /// </summary>
        public string Street { get; set; }
        /// <summary>
        /// Номер дома
        /// </summary>
        public string House { get; set; }
        /// <summary>
        /// Номер подъезда
        /// </summary>
        public string Porch { get; set; }
        /// <summary>
        /// Этаж
        /// </summary>
        public string Floor { get; set; }
        /// <summary>
        /// Квартира или офис
        /// </summary>
        public string Apartment { get; set; }
        /// <summary>
        /// Комментарий к заказу
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Время заказа
        /// </summary>
        public string Date { get; set; }
        /// <summary>
        /// Способ оплаты
        /// </summary>
        public string Pay { get; set; }
        /// <summary>
        /// Подготовка к отправка заказа
        /// </summary>
        /// <param name="cookie">Данные куки</param>
        public async void SendOrder(Models.Cookie cookie)
        {
            Date date = new Date();
            String[] product = cookie.Ids.Split(',');
            String[] product_kol = cookie.Counts.Split(',');

            Dictionary<string, string> param = new Dictionary<string, string>(11);
            param["secret"] = "2bhDGziZHfR44EtHKSRGi2Dh69daTShGTt9nShanrRdfhARTdtnreSBaREbAhk65F6ekbeBtZED64fQ7B4kGHQHkfKKrZFhsArEDz5ZfzkantQiYFAsba3rfA4AiyGDYZ6kf28niirBE6KkB8FD9tSeTGE559yQZr36b2fn8AQDtnQTkhiiD4shAYbsYz6GNTBRGH6E7H6r2H89Sfih9DsAkyR7SHDts492A5syBkNkYHkDSzKS35danT7";
            param["name"] = Name;
            param["mail"] = Email;
            param["phone"] = Phone;
            param["street"] = Street;
            param["home"] = House;
            param["pod"] = Porch;
            param["et"] = Floor;
            param["apart"] = Apartment;
            param["descr"] = Comment;
            param["datetime"] = date.GetDateFrontpad(int.Parse(cookie.Day)) + Date;
            param["pay"] = Pay;

            string data = null;
            foreach(KeyValuePair<string, string> item in param)
            {
                data += "&" + item.Key + '=' + item.Value;
            }

            for(int i = 0;i < product.Length;i++)
            {
                data += "&product[" + i + "]=" + product[i] + "";
                data += "&product_kol[" + i + "]=" + product_kol[i] + "";
            }
            await SendInFrontPad("https://app.frontpad.ru/api/index.php?new_order", data);
        }
        /// <summary>
        /// Отправка заказа в интерфейс frontpad
        /// </summary>
        /// <param name="typeURL">ссылка куда отправить</param>
        /// <param name="data">данные отправления</param>
        /// <returns></returns>
        private async Task<string> SendInFrontPad(string typeURL,string data)
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
    }
}
