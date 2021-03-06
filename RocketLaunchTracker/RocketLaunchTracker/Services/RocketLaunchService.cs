﻿using Newtonsoft.Json;
using RocketLaunchTracker.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RocketLaunchTracker.Services
{
    public class RocketLaunchService
    {
        private const string apiBaseUrl = "https://launchlibrary.net/1.4/";

        public async Task<LaunchInfo> GetNextLaunchesAsync(int count)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(apiBaseUrl);

                var resultString = await httpClient.GetStringAsync($"launch/next/{count}");
                var launchInfo = JsonConvert.DeserializeObject<LaunchInfo>(resultString);

                return launchInfo;
            }
        }

        public async Task<Launch> GetLaunchAsync(int id)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(apiBaseUrl);

                var resultString = await httpClient.GetStringAsync($"launch/{id}");
                var launch = JsonConvert.DeserializeObject<LaunchInfo>(resultString);

                return launch.launches.First();
            }
        }

        public async Task SendReminderNotificationEmailAsync(int launchId)
        {
            var launchObject = await GetLaunchAsync(launchId);
            string liveVideoEmbeddedUrl = ConvertToEmbeddedLink(launchObject.vidURLs?.First()?.ToString());

            var emailBody = new StringBuilder();
            emailBody.AppendLine("Rocket launch info");
            emailBody.AppendLine();
            emailBody.AppendLine($"Name: {launchObject.name}");
            emailBody.AppendLine($"Rocket: {launchObject.rocket.name}");
            emailBody.AppendLine($"Location: {launchObject.location.name}");
            emailBody.AppendLine($"Time: {launchObject.windowstart}");
            emailBody.AppendLine();
            emailBody.AppendLine($"Live URL: {liveVideoEmbeddedUrl}");

            var emailSubject = $"Rocket launch alert from {launchObject.name}";

            SendEmail(emailSubject, emailBody.ToString());
        }

        public async Task SendDelayedNotificationEmailAsync(int launchId)
        {
            var launchObject = await GetLaunchAsync(launchId);
            string liveVideoEmbeddedUrl = ConvertToEmbeddedLink(launchObject.vidURLs?.First()?.ToString());
            var delayedLaunchDate = Convert.ToDateTime(launchObject.windowstart.Replace("UTC", string.Empty).Trim()).AddDays(9);

            var emailBody = new StringBuilder();
            emailBody.AppendLine("Delayed Rocket Launch Info");
            emailBody.AppendLine();
            emailBody.AppendLine($"Name: {launchObject.name}");
            emailBody.AppendLine($"Rocket: {launchObject.rocket.name}");
            emailBody.AppendLine($"Location: {launchObject.location.name}");
            emailBody.AppendLine();
            emailBody.AppendLine("  --------------------------------------------------------------------");
            emailBody.AppendLine($"|  Delayed Time: {delayedLaunchDate.ToLongDateString()} {delayedLaunchDate.ToLongTimeString()} |");
            emailBody.AppendLine("  --------------------------------------------------------------------");
            emailBody.AppendLine();
            emailBody.AppendLine($"Live URL: {liveVideoEmbeddedUrl}");

            var emailSubject = $"Delayed rocket launch alert from {launchObject.name}";

            SendEmail(emailSubject, emailBody.ToString());
        }

        private string ConvertToEmbeddedLink(string link)
        {
            if (link == null)
            {
                return string.Empty;
            }

            const string pattern = "?v=";
            int videoIdIndex = link.IndexOf(pattern) + pattern.Length;

            string videoId = link.Substring(videoIdIndex, link.Length - videoIdIndex);

            return $"https://www.youtube.com/embed/{videoId}";
        }

        private void SendEmail(string subject, string body)
        {
            try
            {
                var client = new SmtpClient("smtp.gmail.com")
                {
                    UseDefaultCredentials = false,
                    //Please don't mess up with this email account. Grately appreciated.
                    Credentials = new NetworkCredential("rocketpathspaceapps@gmail.com", "SpaceAppsChallenge2018"),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("rocketpathspaceapps@gmail.com");
                mailMessage.To.Add("philipcotan@yahoo.com");
                mailMessage.Body = body;
                mailMessage.Subject = subject;
                client.Send(mailMessage);
            }
            catch (Exception e)
            {
                var message = e.Message;
                //log error
            }
        }

        public async Task<List<SpacePort>> GetSpacePorts()
        {
            var jsonString = @"[
    {
      ""country"": ""Algeria"",
      ""location"": ""Centre interarmées d'essais d'engins spéciaux (CIEES), Hammaguir"",
      ""coordinates"":{ ""latitude"": ""31.09951"", ""longitude"": ""-2.83581""},
      ""operational_date"": { ""start_year"": ""1947"", ""end_year"": ""1967""},
      ""rocket_launches"": 230,
      ""heaviest_rocket"": ""18 000 kg"",
      ""highest_altitude"": ""Orbital"",
      ""notes"": ""Operated by France.""
    },
    {
      ""country"": ""Algeria"",
      ""location"": ""Reggane"",
      ""coordinates"":{""latitude"": ""26.71895"", ""longitude"": ""0.27691""},
      ""operational_date"": {""start_year"": ""1961"", ""end_year"": ""1965""},
      ""rocket_launches"": 10,
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Congo (Zaire)"",
      ""location"": ""Shaba North, Kapani Tonneo OTRAG Launch Center"",
      ""coordinates"":{""latitude"": ""-7.92587"", ""longitude"": ""28.52766""},
      ""operational_date"": {""start_year"": ""1977"", ""end_year"": ""1978""},
      ""rocket_launches"": 3,
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""<50 km"",
      ""notes"": ""German OTRAG rockets.""
    },
    {
      ""country"": ""Egypt"",
      ""location"": ""Jabal Hamzah ballistic missile test and launch facility"",
      ""coordinates"":{""latitude"": ""30.125750"", ""longitude"": ""30.605139""},
      ""operational_date"": {""start_year"": ""Late 1950s"", ""end_year"": """"},
      ""rocket_launches"": 6,
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": ""Al Zafir and Al Kahir SRBMs testing""
    },
    {
      ""country"": ""Kenya"",
      ""location"": ""Broglio Space Centre (San Marco), Malindi"",
      ""coordinates"":{""latitude"": ""-2.94080"", ""longitude"": ""40.21340""},
      ""operational_date"": {""start_year"": ""1964"", ""end_year"": ""1988""},
      ""rocket_launches"": 27,
      ""heaviest_rocket"": ""20 000 kg"",
      ""highest_altitude"": ""Orbital"",
      ""notes"": ""Scout rockets, operated by ASI and Sapienza University of Rome, Italy.""
    },
    {
      ""country"": ""Mauritania"",
      ""location"": ""Nouadhibou"",
      ""coordinates"":{""latitude"": ""20.92856"", ""longitude"": ""-17.03153""},
      ""operational_date"": {""start_year"": ""1973"", ""end_year"": ""1973""},
      ""rocket_launches"": 1,
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": ""During a solar eclipse""
    },
     {
      ""country"": ""China"",
      ""location"": ""Base 603, Shijiedu, Guangde"",
      ""coordinates"":{""latitude"": ""30.93743"", ""longitude"": ""119.20575""},
      ""operational_date"": {""start_year"": ""1960"", ""end_year"": ""1966""},
      ""rocket_launches"": """",
      ""heaviest_rocket"": ""1 000 kg"",
      ""highest_altitude"": ""<60 km"",
      ""notes"": """"
    },
    {
      ""country"": ""China"",
      ""location"": ""Jiuquan Satellite Launch Center"",
      ""coordinates"":{""latitude"": ""41.11803"", ""longitude"": ""100.46330""},
      ""operational_date"": {""start_year"": ""1970"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": ""464 000 kg"",
      ""highest_altitude"": ""Orbital"",
      ""notes"": ""Human spaceflight""
    },
    {
      ""country"": ""China"",
      ""location"": ""Taiyuan Satellite Launch Center"",
      ""coordinates"":{""latitude"": ""39.14321"", ""longitude"": ""111.96741""},
      ""operational_date"": {""start_year"": ""1980"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""Orbital"",
      ""notes"": ""Polar satellites""
    },
    {
      ""country"": ""China"",
      ""location"": ""Xichang Satellite Launch Center"",
      ""coordinates"":{""latitude"": ""28.24646"", ""longitude"": ""102.02814""},
      ""operational_date"": {""start_year"": ""1984"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""Lunar"",
      ""notes"": ""Geo-synchronous satellites, lunar probes.""
    },
    {
      ""country"": ""China"",
      ""location"": ""Wenchang Satellite Launch Center"",
      ""coordinates"":{""latitude"": ""19.6144917"", ""longitude"": ""110.9511333""},
      ""operational_date"": {""start_year"": ""2016"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": ""879 000 kg"",
      ""highest_altitude"": ""Orbital"",
      ""notes"": ""New site on Hainan Island with pads for Long March 5 and Long March 7 rockets""
    },
    {
      ""country"": ""China"",
      ""location"": ""Jingyu"",
      ""coordinates"":{""latitude"": ""42.0"", ""longitude"": ""126.5""},
      ""operational_date"": """",
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""India"",
      ""location"": ""Satish Dhawan Space Centre (Sriharikota), Andhra Pradesh"",
      ""coordinates"":{""latitude"": ""13.73740"", ""longitude"": ""80.23510""},
      ""operational_date"": {""start_year"": ""1971"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": ""402 000 kg"",
      ""highest_altitude"": ""Interplanetary"",
      ""notes"": ""Satellites and lunar probes;""
    },
    {
      ""country"": ""India"",
      ""location"": ""Abdul Kalam Island,  Balasore, Odisha"",
      ""coordinates"":{""latitude"": ""20.75804"", ""longitude"": ""87.085533""},
      ""operational_date"": """",
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Indonesia"",
      ""location"": ""Stasiun Peluncuran Roket, Pameungpeuk, Garut"",
      ""coordinates"":{""latitude"": ""-7.646643"", ""longitude"": ""107.689018""},
      ""operational_date"": {""start_year"": ""1965"", ""end_year"": """"},
      ""rocket_launches"": ""14+"",
      ""heaviest_rocket"": ""765 kg"",
      ""highest_altitude"": ""100 km"",
      ""notes"": """"
    },
    {
      ""country"": ""Iran"",
      ""location"": ""Semnan spaceport"",
      ""coordinates"":{""latitude"": ""35.234631"", ""longitude"": ""53.920941""},
      ""operational_date"": {""start_year"": ""2009"", ""end_year"": """"},
      ""rocket_launches"": ""2"",
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""Orbital"",
      ""notes"": """"
    },
    {
      ""country"": ""Israel"",
      ""location"": ""Palmachim Air Force Base"",
      ""coordinates"":{""latitude"": ""31.88484"", ""longitude"": ""34.68020""},
      ""operational_date"": {""start_year"": ""1987"", ""end_year"": """"},
      ""rocket_launches"": ""9"",
      ""heaviest_rocket"": ""70 000 kg"",
      ""highest_altitude"": ""Orbital"",
      ""notes"": """"
    },
    {
      ""country"": ""Japan"",
      ""location"": ""Akita Rocket Range"",
      ""coordinates"":{""latitude"": ""39.57148"", ""longitude"": ""140.05785""},
      ""operational_date"": {""start_year"": ""1956"", ""end_year"": ""1990""},
      ""rocket_launches"": ""81"",
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""343 km"",
      ""notes"": """"
    },
    {
      ""country"": ""Japan"",
      ""location"": ""Uchinoura Space Center"",
      ""coordinates"":{""latitude"": ""31.25186"", ""longitude"": ""131.07914""},
      ""operational_date"": {""start_year"": ""1962"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": ""139 000 kg"",
      ""highest_altitude"": ""Interplanetary"",
      ""notes"": """"
    },
    {
      ""country"": ""Japan"",
      ""location"": ""Tanegashima Space Center, Tanegashima Island"",
      ""coordinates"":{""latitude"": ""30.39096"", ""longitude"": ""130.96813""},
      ""operational_date"": {""start_year"": ""1967"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": ""445 000 kg"",
      ""highest_altitude"": ""Interplanetary"",
      ""notes"": """"
    },
    {
      ""country"": ""Japan"",
      ""location"": ""Ryori"",
      ""coordinates"":{""latitude"": ""39.03000"", ""longitude"": ""141.83000""},
      ""operational_date"": {""start_year"": ""1970"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Japan"",
      ""location"": ""Niijima (ja)"",
      ""coordinates"":{""latitude"": ""34.33766"", ""longitude"": ""139.26575""},
      ""operational_date"": """",
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Japan"",
      ""location"": ""Obachi"",
      ""coordinates"":{""latitude"": ""40.70342"", ""longitude"": ""141.36938""},
      ""operational_date"": """",
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Kazakhstan"",
      ""location"": ""Baikonur Cosmodrome, Tyuratam"",
      ""coordinates"":{""latitude"": ""45.95515"", ""longitude"": ""63.35028""},
      ""operational_date"": {""start_year"": ""1957"", ""end_year"": """"},
      ""rocket_launches"": "">1000"",
      ""heaviest_rocket"": ""2 400 000 kg"",
      ""highest_altitude"": ""Interplanetary"",
      ""notes"": ""First satellite, first human. Operated by Russia.""
    },
    {
      ""country"": ""Kazakhstan"",
      ""location"": ""Sary Shagan"",
      ""coordinates"":{""latitude"": ""46.38000"", ""longitude"": ""72.87000""},
      ""operational_date"": {""start_year"": ""1958"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Korea, North"",
      ""location"": ""Musudan-ri"",
      ""coordinates"":{""latitude"": ""40.85572"", ""longitude"": ""129.66587""},
      ""operational_date"": {""start_year"": ""1998"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": ""Military rockets; satellite launch""
    },
    {
      ""country"": ""Korea, North"",
      ""location"": ""Sohae"",
      ""coordinates"":{""latitude"": ""39.660"", ""longitude"": ""124.705""},
      ""operational_date"": {""start_year"": ""2012"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": ""Military rockets; satellite launch""
    },
    {
      ""country"": ""Korea South"",
      ""location"": ""Anhueng"",
      ""coordinates"":{""latitude"": ""36.70211"", ""longitude"": ""126.47158""},
      ""operational_date"": {""start_year"": ""1993"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Maldives"",
      ""location"": ""Gan Island"",
      ""coordinates"":{""latitude"": ""-0.69328"", ""longitude"": ""73.15672""},
      ""operational_date"": """",
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": ""Several rockets of the Kookaburra type were launched from a pad at 0°41' S and 73°9' E""
    },
    {
      ""country"": ""Russia"",
      ""location"": ""Kheysa"",
      ""coordinates"":{""latitude"": ""80.45000"", ""longitude"": ""58.05000""},
      ""operational_date"": {""start_year"": ""1956"", ""end_year"": ""1980""},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },

    {
      ""country"": ""Russia"",
      ""location"": ""Sovetskaya Gavan"",
      ""coordinates"":{""latitude"": ""48.97000"", ""longitude"": ""140.30000""},
      ""operational_date"": {""start_year"": ""1963"", ""end_year"": ""1964""},
      ""rocket_launches"": ""6"",
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""402 km"",
      ""notes"": """"
    },
    {
      ""country"": ""Russia"",
      ""location"": ""Okhotsk"",
      ""coordinates"":{""latitude"": ""59.367"", ""longitude"": ""143.250""},
      ""operational_date"": {""start_year"": ""1981"", ""end_year"": ""2005""},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""1000 km"",
      ""notes"": """"
    },
    {
      ""country"": ""Russia"",
      ""location"": ""Yasny Cosmodrome (formerly Dombarovskiy), Orenburg Oblast"",
      ""coordinates"":{""latitude"": ""51.20706"", ""longitude"": ""59.85003""},
      ""operational_date"": {""start_year"": ""2006"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": ""211 000 kg"",
      ""highest_altitude"": ""Orbital"",
      ""notes"": ""ICBM base converted for satellites""
    },
    {
      ""country"": ""Taiwan"",
      ""location"": ""Haiqian"",
      ""coordinates"":{""latitude"": ""22.10"", ""longitude"": ""120.90""},
      ""operational_date"": {""start_year"": ""1988"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": ""10 000 kg"",
      ""highest_altitude"": ""300 km"",
      ""notes"": ""Science and technology development""
    },
     {
      ""country"": ""France"",
      ""location"": ""Ile du Levant"",
      ""coordinates"":{""latitude"": ""43.04507"", ""longitude"": ""6.47887""},
      ""operational_date"": {""start_year"": ""1948"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Germany"",
      ""location"": ""Rocket Launch Site Berlin, Berlin-Tegel"",
      ""coordinates"":{""latitude"": ""52.35000"", ""longitude"": ""13.21000""},
      ""operational_date"": {""start_year"": ""1930"", ""end_year"": ""1933""},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""4 km"",
      ""notes"": """"
    },
    {
      ""country"": ""Germany"",
      ""location"": ""Peenemünde/Greifswalder Oie"",
      ""coordinates"":{""latitude"": ""54.14300"", ""longitude"": ""13.79400""},
      ""operational_date"": {""start_year"": ""1942"", ""end_year"": ""1945""},
      ""rocket_launches"": "">3000"",
      ""heaviest_rocket"": ""12 500 kg"",
      ""highest_altitude"": "">100 km"",
      ""notes"": ""V-2 rockets during World War II, first rocket to reach space 20 June 1944""
    },
    {
      ""country"": ""Germany"",
      ""location"": ""Cuxhaven"",
      ""coordinates"":{""latitude"": ""53.84884"", ""longitude"": ""8.59154""},
      ""operational_date"": {""start_year"": ""1945"", ""end_year"": ""1964""},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Germany"",
      ""location"": ""Hespenbusch, Großenkneten"",
      ""coordinates"":{""latitude"": ""52.939002"", ""longitude"": ""8.312515""},
      ""operational_date"": {""start_year"": ""1952"", ""end_year"": ""1957""},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""<10 km"",
      ""notes"": """"
    },
    {
      ""country"": ""Germany"",
      ""location"": ""Zingst"",
      ""coordinates"":{""latitude"": ""54.44008"", ""longitude"": ""12.78431""},
      ""operational_date"": {""start_year"": ""1970"", ""end_year"": ""1992""},
      ""rocket_launches"": ""67"",
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""80 km"",
      ""notes"": """"
    },
    {
      ""country"": ""Greece"",
      ""location"": ""Koroni"",
      ""coordinates"":{""latitude"": ""36.7698"", ""longitude"": ""21.9316""},
      ""operational_date"": {""start_year"": ""1966"", ""end_year"": ""1989""},
      ""rocket_launches"": ""371"",
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""114 km"",
      ""notes"": """"
    },
    {
      ""country"": ""Iceland"",
      ""location"": ""Vik"",
      ""coordinates"":{""latitude"": ""63.41891"", ""longitude"": ""-19.00463""},
      ""operational_date"": {""start_year"": ""1964"", ""end_year"": ""1965""},
      ""rocket_launches"": ""2"",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Italy"",
      ""location"": ""Salto di Quirra"",
      ""coordinates"":{""latitude"": ""39.52731"", ""longitude"": ""9.63303""},
      ""operational_date"": {""start_year"": ""1964"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Norway"",
      ""location"": ""Andøya Space Center"",
      ""coordinates"":{""latitude"": ""69.29430"", ""longitude"": ""16.02070""},
      ""operational_date"": {""start_year"": ""1962"", ""end_year"": """"},
      ""rocket_launches"": "">1200"",
      ""heaviest_rocket"": ""800 kg"",
      ""highest_altitude"": """",
      ""notes"": ""Rockets to the upper atmosphere.""
    },
    {
      ""country"": ""Norway"",
      ""location"": ""Marka"",
      ""coordinates"":{""latitude"": ""58.20000"", ""longitude"": ""7.30000""},
      ""operational_date"": {""start_year"": ""1983"", ""end_year"": ""1984""},
      ""rocket_launches"": """",
      ""heaviest_rocket"": ""16 kg"",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Norway"",
      ""location"": ""SvalRak"",
      ""coordinates"":{""latitude"": ""78.2234"", ""longitude"": ""15.6470""},
      ""operational_date"": {""start_year"": ""1997"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Poland"",
      ""location"": ""Tuchola Forest"",
      ""coordinates"":{""latitude"": ""53.61970"", ""longitude"": ""17.98492""},
      ""operational_date"": {""start_year"": ""1944"", ""end_year"": ""1945""},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": ""Nazi-German V-2 rockets""
    },
    {
      ""country"": ""Poland"",
      ""location"": ""Łeba"",
      ""coordinates"":{""latitude"": ""54.76904"", ""longitude"": ""17.59355""},
      ""operational_date"": {""start_year"": ""1941"", ""end_year"": ""1945""},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": ""Nazi-German rockets""
    },
    {
      ""country"": ""Poland"",
      ""location"": ""Łeba-Rąbka"",
      ""coordinates"":{""latitude"": ""54.754486"", ""longitude"": ""17.517919""},
      ""operational_date"": {""start_year"": ""1963"", ""end_year"": ""1973""},
      ""rocket_launches"": ""36"",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": ""Polish rockets""
    },
    {
      ""country"": ""Poland"",
      ""location"": ""Blizna"",
      ""coordinates"":{""latitude"": ""50.18190"", ""longitude"": ""21.61620""},
      ""operational_date"": {""start_year"": ""1943"", ""end_year"": ""1944""},
      ""rocket_launches"": ""139"",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": ""Nazi-German V-2 rockets""
    },
    {
      ""country"": ""Russia"",
      ""location"": ""Kapustin Yar Cosmodrome, Astrakhan Oblast"",
      ""coordinates"":{""latitude"": ""48.57807"", ""longitude"": ""46.25420""},
      ""operational_date"": {""start_year"": ""1957"", ""end_year"": """"},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": ""Orbital"",
      ""notes"": ""Previously for satellite launches""
    },
    {
      ""country"": ""Russia"",
      ""location"": ""Nyonoksa"",
      ""coordinates"":{""latitude"": ""64.64928"", ""longitude"": ""39.18721""},
      ""operational_date"": {""start_year"": ""1965"", ""end_year"": ""1997""},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Russia"",
      ""location"": ""Plesetsk Cosmodrome"",
      ""coordinates"":{""latitude"": ""62.92556"", ""longitude"": ""40.57778""},
      ""operational_date"": {""start_year"": ""1966"", ""end_year"": """"},
      ""rocket_launches"": "">1000"",
      ""heaviest_rocket"": ""760 000 kg"",
      ""highest_altitude"": ""Orbital"",
      ""notes"": """"
    },
    {
      ""country"": ""Spain"",
      ""location"": ""El Arenosillo"",
      ""coordinates"":{""latitude"": ""37.09687"", ""longitude"": ""-6.73863""},
      ""operational_date"": {""start_year"": ""1966"", ""end_year"": """"},
      ""rocket_launches"": "">500"",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Sweden"",
      ""location"": ""Nausta"",
      ""coordinates"":{""latitude"": ""66.357202"", ""longitude"": ""19.275813""},
      ""operational_date"": {""start_year"": ""1961"", ""end_year"": ""1961""},
      ""rocket_launches"": ""1"",
      ""heaviest_rocket"": ""30 kg"",
      ""highest_altitude"": ""<80 km"",
      ""notes"": ""Arcas rocket for atmospheric research.""
    },
    {
      ""country"": ""Sweden"",
      ""location"": ""Kronogård"",
      ""coordinates"":{""latitude"": ""66.4147"", ""longitude"": ""19.2767""},
      ""operational_date"": {""start_year"": ""1961"", ""end_year"": ""1964""},
      ""rocket_launches"": ""18"",
      ""heaviest_rocket"": ""700 kg"",
      ""highest_altitude"": ""135 km"",
      ""notes"": ""Arcas, Nike-Cajun and Nike-Apache rockets for atmospheric research.""
    },
    {
      ""country"": ""Canada"",
      ""location"": ""Southend"",
      ""coordinates"":{""latitude"": ""56.333"", ""longitude"": ""-103.233""},
      ""operational_date"": {""start_year"": ""1980"", ""end_year"": ""1980""},
      ""rocket_launches"": ""2"",
      ""heaviest_rocket"": ""1 200 kg"",
      ""highest_altitude"": """",
      ""notes"": """"
    },
    {
      ""country"": ""Greenland (Denmark)"",
      ""location"": ""Thule Air Base"",
      ""coordinates"":{""latitude"": ""76.4240"", ""longitude"": ""-68.2936""},
      ""operational_date"": {""start_year"": ""1964"", ""end_year"": ""1980""},
      ""rocket_launches"": """",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": ""US Air Force""
    },
    {
      ""country"": ""United States"",
      ""location"": ""White Sands Missile Range"",
      ""coordinates"":{""latitude"": ""32.56460"", ""longitude"": ""-106.35908""},
      ""operational_date"": {""start_year"": ""1946"", ""end_year"": """"},
      ""rocket_launches"": "">7000"",
      ""heaviest_rocket"": """",
      ""highest_altitude"": """",
      ""notes"": ""Military and civilian flights. Served as alternate landing site for the space shuttle.""
    }
   ]";
            
            var spacePorts = JsonConvert.DeserializeObject<List<SpacePort>>(jsonString);

            return spacePorts;
        }

     
    }
}
