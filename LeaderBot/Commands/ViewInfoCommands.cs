using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text;
using MongoDB.Driver;

namespace LeaderBot {
	public class ViewInfoCommands : ModuleBase {
        SupportingMethods methods = new SupportingMethods();

		public ViewInfoCommands() {
		}

		[Command("help"), Summary("Get's a list of all the commands"),Alias("Commands","Commandlist")]
		public async Task showCommands() {
			await ReplyAsync("`-help`, shows all available commands\n" +
								"`-rolecount [User]`, gets your current amount of roles if no user is specified, otherwise returns rolecount of user specified.\n" +
								"`-leaderboardRole [number of entries shown]`, returns a leaderboard of users with the most roles. Defaults to the top 10 users.\n" +
                                "`-leaderboardexperience [number of entries shown]`, returns a leaderboard of users with the most exp. Defaults to the top 10 users.\n" +
                                "`-missingroles`, returns a list of roles the user does not currently have\n" +
								"`-getroledesc <role name>`, returns the description of the role and how to obtain\n" +
								"`-admin`\n" +
								"\t`-admin giverole <user that receives role> <role to give>`, gives a role to a user\n" +
								"\t`-admin createroles`, currently adds roles from json format, but will be changed later to have more input");
		}

		[Command("rolecount"), Summary("Gets the role count of the current user")]
		public async Task roleCount([Summary("user to get role of. Defaults to user who sent the message if no user is specified.")] SocketGuildUser user = null) {

			try {
				if (user == null) {
					user = Context.Message.Author as SocketGuildUser;
				}
				int amountOfRoles = user.Roles.Count - 1;
				await ReplyAsync($"{user} has {amountOfRoles} roles");

			} catch (Exception ex) {
				await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".roleCount", "Unexpected Exception", ex));
			}
		}

		[Command("leaderboardRole"), Summary("Gets the role leaderboard")]
		public async Task getRoleLeaderboard([Summary("Places to see on leaderboard")] int roleCount = 10) {
            StringBuilder stringBuilder = SupportingMethods.boardtitlecard("Roles");
            try {
				var allUsers = new Dictionary<SocketGuildUser, int>();

				foreach (var user in (await Context.Guild.GetUsersAsync())) {
                    if (!user.IsBot){
                        allUsers.Add(user as SocketGuildUser, ((SocketGuildUser)user).Roles.Count - 1);
                    }
				}
                StringBuilder board = SupportingMethods.createLeaderboard(stringBuilder, allUsers, roleCount);
                await ReplyAsync($"{board.ToString()}");
            } catch (Exception ex) {
				await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".getRoleLeaderboard", "Unexpected Exception", ex));
			}
		}

		[Command("leaderboardExperience"), Summary("Gets the role leaderboard"),Alias("leadexperienceboard")]
		public async Task getExpLeaderboard([Summary("Places to see on leaderboard")] int userCount = 10) {
            StringBuilder stringBuilder = SupportingMethods.boardtitlecard("Exp");
			try {
				var allUsers = new Dictionary<SocketGuildUser, int>();

				foreach (var user in (await Context.Guild.GetUsersAsync())) {
					if (!user.IsBot) {
						var userName = user as SocketUser;
						UserInfo userInfo = SupportingMethods.getUserInformation(userName.ToString());
                        if (userInfo != null){
                            allUsers.Add(user as SocketGuildUser, userInfo.Experience);
                        }
					}
				}
                StringBuilder board = SupportingMethods.createLeaderboard(stringBuilder, allUsers, userCount);
                await ReplyAsync($"{board.ToString()}");
            } catch (Exception ex) {
				await Logger.Log(new LogMessage(LogSeverity.Error, $"{GetType().Name}.getExpLeaderboard", "Unexpected Exception", ex));
			}
		}


        [Command("echo")]
        public async Task echo(string restOfText = null)
        {
            if (restOfText == null){
                await ReplyAsync("no text specified");
            }
            else
            {
                await ReplyAsync(restOfText);
          }
        }

        [Command("leaderboard")]
        public async Task findleaderboard(String leaderboardname, int usercount = 10)
        {
            if(leaderboardname == "exp")
            {
                await getExpLeaderboard(usercount);
            }
            else if(leaderboardname == "roles")
            {
                await getRoleLeaderboard(usercount);
            }
            else
            {
                await ReplyAsync($"Leaderboard '{leaderboardname}' not found");
            }
        }
        //will change, POC
        //FIXME lol
        [Command("missingRoles"), Summary("Gives a list of currently not attained roles")]
		public async Task missingRoles() {
			List<SocketRole> allGuildRoles = new List<SocketRole>();
			foreach (SocketRole guildRoles in ((SocketGuild) Context.Guild).Roles) {
				allGuildRoles.Add(guildRoles);
			}
			foreach (SocketRole userRole in ((SocketGuildUser) Context.Message.Author).Roles) {
				if (allGuildRoles.Contains(userRole))
					allGuildRoles.Remove(userRole);
			}
            string missingroles = "";
			foreach (var unobtainedRole in allGuildRoles) {
                missingroles += unobtainedRole.ToString() + ", ";
			}
            await ReplyAsync(missingroles);
		}

		[Command("getRoleDesc"), Summary("Returns role description"),Alias("roledesc","getroledescription","roledescription")]
		public async Task getRoleDesc([Summary("The role to get the description for")] string roleName) {
			var selectedRole = Context.Guild.Roles.FirstOrDefault(x => SupportingMethods.stringEquals(x.Name, roleName));
			var allRoles = SupportingMethods.LoadAllRolesFromServer();
			var role = allRoles.Find(x => SupportingMethods.stringEquals(x.Name, selectedRole.Name));
			await ReplyAsync($"To get ***{role.Name}***\n\t-{role.Description}");
		}

		[Command("getExperience"), Summary("Returns user experience"), Alias("exp","myexp","getexp","getmyexp","experience","getcurrentexp","currentexp")]
        public async Task getExperience([Summary("The user to get exp total from")] SocketGuildUser userName = null) {
			try {

                if (userName == null)
                {
                    userName = ((SocketGuildUser)Context.Message.Author);
                }
                var user = userName as SocketUser;
                UserInfo userInfo = SupportingMethods.getUserInformation(user.ToString());
                if (userInfo != null) {
					var currentExp = userInfo.Experience;
					var level = Math.Round(Math.Pow(currentExp, 1 / 1.3) / 100);
					await ReplyAsync($"{user} has {currentExp} experience and is level {level}");

				}
			} catch (Exception ex) {
				await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".getExperience", "Unexpected Exception", ex));
			}
		}

        [Command("MyInfo"), Summary("Returns Player Stat Summary")]
        public async Task getPlayerInfoChat(SocketGuildUser userName = null){
            try
            {

                if (userName == null)
                {
                    userName = ((SocketGuildUser)Context.Message.Author);
                }
                var user = userName as SocketUser;
                UserInfo userInfo = SupportingMethods.getUserInformation(user.ToString());
                if (userInfo != null)
                {
                    var currentExp = userInfo.Experience;
                    var level = Math.Round(Math.Pow(currentExp, 1 / 1.3) / 100);
                    var messages = userInfo.NumberOfMessages;
                    var reactions = userInfo.ReactionCount;
                    var date = userInfo.DateJoined;
                    await ReplyAsync($"{user} has \n-Exp:{currentExp} \n-Level:{level} \n-MessageCount:{messages} \n-Reactions:{reactions} \n-Joined:{date}");

                }
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".getPlayerInfoChat", "Unexpected Exception", ex));
            }
        }
	}
}
