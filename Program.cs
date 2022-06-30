using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Task13ConsoleAPI
{
    internal class Program
    {
        public static string APIKey = "69bbc9286329c22bc01d2b4040bd870d";
        public static HttpClient client = new HttpClient();
        public static List<City> Cities { get; set; }

        static void Main(string[] args)
        {
            Cities = LoadCitiesData();

            MainMenu().GetAwaiter().GetResult();
        }

        public static List<City> LoadCitiesData()
        {
            var list = new List<City>();
            Csv csv = new Csv();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "pk.csv");
            csv.FileOpen(path);
            var objs = csv.Rows[1];
            for (int i = 1; i < csv.Rows.Count(); i++)
                list.Add(new City() { CityName= csv.Rows[i][0],Latitude= Convert.ToDouble(csv.Rows[i][1]), Longitude = Convert.ToDouble(csv.Rows[i][2]), Province = csv.Rows[i][5] });
            return list;
        }

        public static async Task MainMenu()
        {
            Console.WriteLine("\n\n");
            for (int i = 0; i < Cities.Count(); i++)
                Console.WriteLine(i+1 + "\t" + Cities[i].CityName + "\t\t" + Cities[i].Latitude + "\t" + Cities[i].Longitude + "\t" + Cities[i].Province);

            Console.WriteLine("\n\n");

            Console.WriteLine("Select a City");
            int num=0;
            string input = Console.ReadLine();
            try
            {
                num = Convert.ToInt32(input);
            }catch(Exception e)
            {
                Console.WriteLine("Invalid Input");
                await MainMenu();
            }

            if(num>0 && num < Cities.Count())
            {
                await WheatherAPI(Cities[num - 1]);
            }
            await MainMenu();

        }
        


        static async Task WheatherAPI(City city)
        {
            client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                HttpResponseMessage response = await client.GetAsync($"weather?lat={city.Latitude}&lon={city.Longitude}&appid={APIKey}");
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()));
                }else if (response.StatusCode== HttpStatusCode.NotFound)
                {
                    Console.WriteLine("Not found");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press any key to Continue....");
            Console.ReadLine();
        }
    }
}
