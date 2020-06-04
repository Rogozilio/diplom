﻿using bdtest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;

namespace bdtest.Functions
{
    public class Products
    {
        private ProductContext _base;
        public Products()
        {
            ProductContext dataBase = new ProductContext();
            _base = dataBase; 
        }

        public List<string> GetKind(int day)
        {
            List<string> kinds = new List<string> { "Первые блюда",
                        "Вторые блюда", "Овощные блюда и гарниры", "Салаты и закуски",
                        "Десерты, выпечка", "Блины", "Бутерброды", "Напитки",
                        "Диетическое питание", "Праздничные блюда", "Соусы и приправы",
                        "Хлеб", "Одноразовая посуда"};
            List<string> allKind = new List<string>();
            if (day > 0 && day < 6)
                allKind.Add("Комплексные обеды");
            foreach(var kind in kinds.ToList())
                foreach (var product in _base.products.ToList())
                {
                    if (product.Day.Contains(day.ToString()) && kind == product.Kind)
                    {
                        allKind.Add(product.Kind);
                        break;
                    }
                }
            return allKind;
        }
        public List<string> GetKomplexKind(int day, bool extra = false)
        {
            List<String> list = GetKind(day);
            list.RemoveAt(0);
            list = new List<string>(list.Distinct());
            if (extra)
                list.RemoveRange(0, list.IndexOf("Салаты и закуски"));
            else
                list = list.GetRange(0, list.IndexOf("Салаты и закуски"));
            return list;
        }
        public List<Product> GetProductOnDayAndKind(int day, string kind)
        {
            List<Product> list = new List<Product>();
            foreach (var product in _base.products.ToList())
            {
                if(product.Day.Contains(day.ToString()) && product.Kind.Contains(kind))
                {
                    list.Add(product);
                }
            }
            return list;
        }
        public List<Product> GetKomplexOnDayAndKind(int day, string kind)
        {
            List<Product> list = new List<Product>();
            foreach (var product in _base.products.ToList())
            {
                if (product.Komplex.Contains(day.ToString()) && product.Kind.Contains(kind))
                {
                    list.Add(product);
                }
            }
            return list;
        }
        public string[] GetProductOnId(string cookieId, string cookieCount)
        {
            if (cookieId == null || cookieCount == null)
            {
                return null;
            }
            int i = 0;
            int j = 0;
            bool isKomplex = false;
            string[] count = cookieCount.Split(',');
            string[] list = new string[count.Length*5];
            foreach (var cookie in cookieId.Split(','))
                foreach (var product in _base.products.ToList())
                {
                    if(product.Id.ToString() == cookie)
                    { 
                        if (!isKomplex)
                        {
                            list[i++] = product.Id.ToString();
                            list[i++] = product.Image;
                            list[i++] = product.Name;
                            list[i++] = product.Price.ToString();
                            list[i++] = count[j++].ToString();
                            if (cookie == 2.ToString())
                            {
                                list[i - 3] += ':';
                                isKomplex = true;
                            }
                        }
                        else
                        {
                            list[i-5] +=',' + product.Id.ToString();
                            list[i-3] +="<br> -" + product.Name;
                            if (cookie == 348.ToString())
                                isKomplex = false;
                        }
                        break;
                    }
                }
            return list;
        }
        public int GetPriceProducs(string cookieId, string cookieCount, bool isDelivery = true)
        {
            if (cookieId == null || cookieCount == null)
            {
                return 0;
            }
            int j = 0;
            int price = 0;
            string[] count = cookieCount.Split(',');
            string[] product = GetProductOnId(cookieId, cookieCount);
            for(int i = 3; i < product.Length;i=i+5)
            {
                price += int.Parse(product[i]) * int.Parse(count[j++]);
            }
            if (isDelivery)
                return (price < 300) ? price + 50 : price;
            else
                return price;
        }
    }
}
