using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using AngleSharp;
using AngleSharp.Dom;

namespace HomeFinder.Controllers.Parsers
{
    public class ParserDomofond : Parser
    {
        readonly string source;

        public ParserDomofond()
        {
            source = "https://www.domofond.ru";
        }

        public override async Task<List<RentalOffer>> ParseByUrl(string url)
        {
            // Формируем контент страницы:
            WebRequest request = WebRequest.Create(url);
            Stream stream = request.GetResponse().GetResponseStream();
            string content = new StreamReader(stream).ReadToEnd();

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(content));

            // Находим количество страниц, которые надо просмотреть, чтобы запарсить все результаты поиска:
            var list = document.QuerySelectorAll("li").Where(li =>
            li.ClassName == "pagination__page___2dfw0").ToList();
            int pageCount = int.Parse(list.Last().GetAttribute("data-marker").Remove(0, 16));


            // Собираем результаты поиска со всех страниц в одну коллекцию:
            var searchResults = new List<IElement>();
            for (int i = 1; i <= pageCount; i++)
            {
                string curUrl = url + $"&Page={i}";

                request = WebRequest.Create(curUrl);
                stream = request.GetResponse().GetResponseStream();
                content = new StreamReader(stream).ReadToEnd();

                document = await context.OpenAsync(req => req.Content(content));

                var curList = document.QuerySelectorAll("a").Where(a =>
                a.ClassName == "long-item-card__item___ubItG search-results__itemCardNotFirst___3fei6" ||
                a.ClassName == "long-item-card__item___ubItG long-item-card__seenItem___3t2Fz"
                ).ToList();

                searchResults.AddRange(curList);
            }


            // Достаём нужную нам информацию:
            var allOffers = new List<RentalOffer>();
            foreach (var oneResult in searchResults)
            {
                string href = oneResult.GetAttribute("href");
                string address = oneResult.QuerySelectorAll("span").Where(s => s.ClassName == "long-item-card__address___PVI5p").First().InnerHtml;
                string description = oneResult.QuerySelectorAll("div").Where(s => s.ClassName == "description__descriptionBlock___3KWc1").First().InnerHtml;

                description = description.Replace("&nbsp;", " ");


                int end = description.IndexOf("</p>");


                description = description.Remove(end, description.Length - end).Remove(0, 3);

                allOffers.Add(new RentalOffer { Address = address, Description = description.Split("<br>", StringSplitOptions.RemoveEmptyEntries), Href = source + href });
            }

            return allOffers;
        }
    }
}
