﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityModel.Client;
using System.Net.Http;
using System.Threading;

namespace LeaderboardLoadTest
{
    public class Program
    {
        public static Dictionary<string, string> users = new Dictionary<string, string>
        {
            {"devuser", "devuser"},
            {"devadmin", "devadmin"}
        };

        private static string baseUrl = "http://localhost:5000";
        private static string route = "/api/leaderboard";

        private static Random s_r = new Random();

        public static void Main(string[] args)
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            MainAsync(token).Wait();
        }

        public static async Task MainAsync(CancellationToken cancellationToken)
        {
            foreach (var userEntry in users)
            {
                // login to the game
                var tokenResponse = await gamerLoginAsync(userEntry);

                // simulate leaderboard activity  
                var task = Task.Factory.StartNew(() => simulateGameAsync(tokenResponse, cancellationToken));
            }
            Console.Read();
        }

        private static async Task simulateGameAsync(TokenResponse tokenResponse, CancellationToken cancellationToken)
        {
            var accessToken = tokenResponse.AccessToken;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int count = s_r.Next(1, 5);
                for (int i = 0; i < count; i++)
                {
                    // send game score (POST)
                    // TODO - handle execption
                    await postScoreAsync(accessToken);
                    Thread.Sleep(s_r.Next(1000, 10000));
                }

                // ask for leaderboard scores (GET)
                // TODO - handle execption
                await getScoresAsync(accessToken);
                Thread.Sleep(s_r.Next(1000, 10000));
            }
        }

        private static async Task getScoresAsync(string accessToken)
        {
            var client = new HttpClient();
            client.SetBearerToken(accessToken);
            var response = await client.GetAsync(baseUrl + route);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to get score: " + response.StatusCode);
            }

            var content = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(content);
        }

        private static async Task postScoreAsync(string accessToken)
        {
            int score = s_r.Next(1500);           

            var client = new HttpClient();
            client.SetBearerToken(accessToken);
            var response = await client.PostAsJsonAsync(baseUrl + route, new { country = "missing", customTag = "testclient", score = score });

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to post score: " + response.StatusCode);
            }
            else
            {
                Console.WriteLine("Post Score: " + score);
            }
        }

        private static async Task<TokenResponse> gamerLoginAsync(KeyValuePair<string, string> userEntry)
        {
            var disco = await DiscoveryClient.GetAsync(baseUrl);

            // request token
            var tokenClient = new TokenClient(disco.TokenEndpoint, "resourceowner-test", "devsecret");
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(userEntry.Key, userEntry.Value, "nether-all");

            return tokenResponse;
        }
    }
}
