using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeFinder.Controllers
{
    public static class Filtrator
    {
        private static string[] StrAbb = { "улица", "ул.", "ул ", "ул,", "переулок", "пер.", "пер ", "пер,", "проезд", "проспект", "пр-кт" };

        public static List<Parsers.RentalOffer> Filtration(List<Parsers.RentalOffer> allOffers, List<Models.Address> addresses)
        {
            List<Parsers.RentalOffer> result = new List<Parsers.RentalOffer>();

            // Обрабатываем каждое предложение:
            foreach (var offer in allOffers)
            {
                // Разбиваем предложение на блоки:
                string[] blocks = offer.Address.ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries);

                string offerStreet = "";

                // Ищем блок с названием улицы
                int indexI = 0;
                int indexJ = 0;
                bool isFind = false;
                for (int i = 0; i < blocks.Length; i++)
                {
                    for (int j = 0; j < StrAbb.Length; j++)
                        if (blocks[i].Contains(StrAbb[j]))
                        {
                            indexJ = j; isFind = true; break;
                        }

                    if (isFind)
                    {
                        indexI = i;
                        offerStreet = blocks[i].Replace(StrAbb[indexJ], "").Trim();
                        break;
                    }
                }

                if (indexI + 1 == blocks.Length)
                    // Предложение плохое, следующее
                    continue;

                // Вытягиваем из следующего блока номер дома и, возможно, номер корпуса
                string offerHouse = "";
                int offerBuilding = 0;

                string[] houseBlocks;
                string nextBlock = blocks[indexI + 1].Replace("д.", "").Trim();
                if (nextBlock.Contains("к"))
                    houseBlocks = nextBlock.Split('к', StringSplitOptions.RemoveEmptyEntries);
                else if (nextBlock.Contains("/"))
                    houseBlocks = nextBlock.Split('/', StringSplitOptions.RemoveEmptyEntries);
                else
                    houseBlocks = new string[] { nextBlock };

                offerHouse = houseBlocks[0];

                if (houseBlocks.Length == 2)
                    // Вытягиваем корпус:
                    int.TryParse(houseBlocks[1], out offerBuilding);
                else
                {
                    // Ищем корпус в следующем блоке:
                    if (indexI + 2 < blocks.Length)
                    {
                        nextBlock = blocks[indexI + 2];
                        nextBlock = nextBlock.Replace("к.", "").Trim();
                        int.TryParse(nextBlock, out offerBuilding);
                    }
                }

                // Получили все необходимые данные, ищем подходящие адреса:
                foreach (var address in addresses)
                {
                    if (address.Street.ToLower().Trim() == offerStreet
                        && address.House.ToLower().Trim() == offerHouse
                        && address.Building == offerBuilding)
                    {
                        // Данное предложение подходит!
                        result.Add(offer);
                    }
                }
            }

            return result;
        }
    }
}
