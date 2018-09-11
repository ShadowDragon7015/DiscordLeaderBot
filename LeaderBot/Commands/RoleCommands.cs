﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace LeaderBot {
	[Group("admin")]
	public class RoleCommands : ModuleBase {
		public static List<Roles> allRoles = LoadJson();
		public Random rand = new Random();

		public RoleCommands() {
		}

		public static List<Roles> LoadJson() {
			List<Roles> allRolesInServer;
			using (StreamReader r = new StreamReader("roles.json")) {
				var json = r.ReadToEnd();
				allRolesInServer = JsonConvert.DeserializeObject<List<Roles>>(json);
			}
			return allRolesInServer;
		}

		//https://docs.stillu.cc/
		[Command("createRoles"), Summary("Creates a role in the guild")]
		public async Task createRoles() {
			LoadJson();
			List<string> currentGuildRoles = new List<string>();
			foreach (SocketRole guildRoles in ((SocketGuild)Context.Guild).Roles) {
				currentGuildRoles.Add(guildRoles.Name);
			}

			foreach (var role in allRoles) {
				if (!currentGuildRoles.Contains(role.Name)) {
					var randColor = new Color(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256));
					await Context.Guild.CreateRoleAsync(role.Name, GuildPermissions.None, randColor);
					await Logger.Log(new LogMessage(LogSeverity.Verbose, GetType().Name + ".createRoles", "Added role to server: " + role.Name));
					await ReplyAsync($"Added role: {role.Name}\nHow to get: {role.Description}");
				}
			}
		}

		[Command("giveRole"), Summary("Adds role to specified user"), RequireUserPermission(GuildPermission.Administrator)]
		public async Task giveRole([Summary("The user to add role to")] SocketGuildUser user, [Summary("The role to add")] string roleName) {

			var userInfo = user as SocketUser;
			var currentGuild = user.Guild as SocketGuild;
			var role = currentGuild.Roles.FirstOrDefault(x => x.Name.ToLower() == roleName.ToLower());
			await Logger.Log(new LogMessage(LogSeverity.Info, GetType().Name + ".addRole", userInfo.ToString() + " added role " + role.ToString()));
			await (userInfo as IGuildUser).AddRoleAsync(role);
		}
	}
}
