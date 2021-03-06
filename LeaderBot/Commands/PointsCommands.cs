﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LeaderBot.Points;

namespace LeaderBot.Commands
{
    public class PointsCommands : ModuleBase
    {
        public PointsCommands()
        {
        }

        [Command("dailyPoints"), Summary("Adds points to user")]
        public async Task dailyPoints()
        {
            SupportingMethods.SetupMongoCollection("pointsReceived");
            string currentDate = DateTime.Now.ToString("yyyyMMdd");
            PointsReceived pointsReceived = SupportingMethods.getPointsReceived("date", currentDate);
            var user = Context.Message.Author;
            List<string> users = pointsReceived.Users.ToList();

            if (users.Contains(user.ToString()))
            {
                SupportingMethods.SetupMongoCollection("userData");
                await ReplyAsync($"{user} has already reclaimed the daily points.");
            }
            else
            {
                SupportingMethods.updateArray("date", currentDate, "users", user.ToString());
                SupportingMethods.SetupMongoCollection("userData");
                Random rand = new Random();
                var points = rand.Next(100, 250);
                SupportingMethods.updateDocument(user.ToString(), "points", points);

                await ReplyAsync($"{user} earned {points} points!");
            }

        }

        [Command("getPoints"), Summary("gets user total points")]
        public async Task getPoints([Summary("The user to get point total from")] SocketGuildUser userName = null)
        {
            if (userName == null)
            {
                userName = ((SocketGuildUser)Context.Message.Author);
            }
            var user = userName as SocketUser;
            UserInfo userInfo = SupportingMethods.getUserInformation(user.ToString());
            if (userInfo != null)
            {
                var currentPoints = userInfo.Points;
                await ReplyAsync($"{user} has {currentPoints} points!");

            }
        }

        [Command("bet"), Summary("bet with user total points")]
        public async Task bet([Summary("Amount of points to bet")] int bettingPoints, [Summary("Side of coin picked.")] string coinSide)
        {
            var userName = ((SocketGuildUser)Context.Message.Author);

            var user = userName as SocketUser;
            UserInfo userInfo = SupportingMethods.getUserInformation(user.ToString());
            if (userInfo != null)
            {
                var currentPoints = userInfo.Points;
                if (bettingPoints > currentPoints)
                {
                    await ReplyAsync($"{user} has {currentPoints} points! You cannot bet {bettingPoints}!");
                }
                else if (bettingPoints < 50)
                {
                    await ReplyAsync($"Minimum bet is 50 points");
                }
                else
                {
                    Random rand = new Random();
                    var num = rand.Next(0,2);
                    string result = null;
                    if (num == 0)
                    {
                        result = "heads";
                    }
                    else
                    {
                        result = "tails";
                    }
                    await ReplyAsync($"Result: {result}");
                    if (SupportingMethods.stringEquals(result, coinSide))
                    {
                        SupportingMethods.updateDocument(userName.ToString(), "points", bettingPoints);
                        await ReplyAsync($"Congratulations, you won!\n{user} has {currentPoints + bettingPoints} points!");
                    }
                    else
                    {
                        SupportingMethods.updateDocument(userName.ToString(), "points", bettingPoints * -1);
                        await ReplyAsync($"You lost!\n{user} has {currentPoints - bettingPoints} points!");
                    }

                }

            }
        }
    }
}
