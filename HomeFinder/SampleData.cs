using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeFinder.Models;

namespace HomeFinder
{
    public class SampleData
    {
        public static void Initialize(HomeContext context)
        {
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        MinCost = 5000,
                        MaxCost = 10000,
                        MinRoomCount = 1,
                        MaxRoomCount = 2
                    },
                    new User
                    {
                        MinCost = 12000,
                        MaxCost = 16000,
                        MinRoomCount = 2,
                        MaxRoomCount = 2
                    },
                    new User
                    {
                        MinCost = 3000,
                        MaxCost = 4000,
                        MinRoomCount = 0,
                        MaxRoomCount = 1
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
