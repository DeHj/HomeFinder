using System;
using System.Collections.Generic;

namespace HomeFinder.Controllers
{
    public static class Filtrator
    {
        public static string[] strAbb = { "улица", "ул.", "ул ", "ул,", "переулок", "пер.", "пер ", "пер,", "проезд", "проспект", "пр-кт" };

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
                    for (int j = 0; j < strAbb.Length; j++)
                        if (blocks[i].Contains(strAbb[j]))
                        {
                            indexJ = j; isFind = true; break;
                        }

                    if (isFind)
                    {
                        indexI = i;
                        offerStreet = blocks[i].Replace(strAbb[indexJ], "").Trim();
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
                    string street = address.Street.ToLower().Trim();
                    string house = address.House.ToLower().Trim();
                    int building = address.Building;

                    if (street == offerStreet
                        && house == offerHouse
                        && building == offerBuilding)
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
