using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Discord.Rest;
using FriendManager.Discord;
using FriendManager.BAL.Discord;
using TL;
using CustomSpectreConsole;
using CustomSpectreConsole.Settings;
using Spectre.Console;

namespace FriendManager.Functions
{
    public class DiscordManager : ManagerBase<DiscordManager>
    {
        #region Properties

        public override string DisplayName => "Discord Manager";

        private FriendBotClient Client;
        private DataExtractionClient ExtractionClient;
        private bool RunningSyncRoutine;
        private bool ScheduledSyncEnabled;
        private bool RunningPurgeRoutine;
        private bool ScheduledPurgeEnabled;
        private bool RunningChannelPermissionsRoutine;

        public string Messages => "[{\"MessageId\":1149934368786235395,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"*e\",\"SentAt\":\"2023-09-09T01:08:38.266-04:00\",\"Attachments\":[]},{\"MessageId\":1149934353535750305,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Okay no don't I'm done byyyyw\",\"SentAt\":\"2023-09-09T01:08:34.63-04:00\",\"Attachments\":[]},{\"MessageId\":1149934218932137984,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"I am really gonna block you lmao\",\"SentAt\":\"2023-09-09T01:08:02.538-04:00\",\"Attachments\":[]},{\"MessageId\":1149933909820317717,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"\",\"SentAt\":\"2023-09-09T01:06:48.84-04:00\",\"Attachments\":[{\"AttachmentType\":\"audio/ogg\",\"DownloadUrl\":\"https://media.discordapp.net/attachments/1084330569141329932/1149933909182775346/voice-message.ogg\"}]},{\"MessageId\":1149933297854586982,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"No I’m still gonna do my tests here lol\",\"SentAt\":\"2023-09-09T01:04:22.936-04:00\",\"Attachments\":[]},{\"MessageId\":1149933117411446875,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Yes but when you’re on the phone it just says ‘J’ in a circle lol\",\"SentAt\":\"2023-09-09T01:03:39.915-04:00\",\"Attachments\":[]},{\"MessageId\":1149933010922242160,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"JK you wanna use this server for yourself?\",\"SentAt\":\"2023-09-09T01:03:14.526-04:00\",\"Attachments\":[]},{\"MessageId\":1149932953049251890,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"GUESS WHICH ONE I AM\",\"SentAt\":\"2023-09-09T01:03:00.728-04:00\",\"Attachments\":[]},{\"MessageId\":1149932931666677810,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"THE SERVER IS LITERALLY CALLED JAYNIK\",\"SentAt\":\"2023-09-09T01:02:55.63-04:00\",\"Attachments\":[]},{\"MessageId\":1149932817703256134,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"GO AWAY\",\"SentAt\":\"2023-09-09T01:02:28.459-04:00\",\"Attachments\":[]},{\"MessageId\":1149932644306534481,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/pov-alex-gif-22598014\",\"SentAt\":\"2023-09-09T01:01:47.118-04:00\",\"Attachments\":[]},{\"MessageId\":1149932474231693363,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"I could be your bot\",\"SentAt\":\"2023-09-09T01:01:06.569-04:00\",\"Attachments\":[]},{\"MessageId\":1149932238172078113,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Nah I got eyes everywhere\",\"SentAt\":\"2023-09-09T01:00:10.288-04:00\",\"Attachments\":[]},{\"MessageId\":1149932173244239952,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"I’m making a bot\",\"SentAt\":\"2023-09-09T00:59:54.808-04:00\",\"Attachments\":[]},{\"MessageId\":1149932107376885781,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"What the hell I thought this was my personal server lmao\",\"SentAt\":\"2023-09-09T00:59:39.104-04:00\",\"Attachments\":[]},{\"MessageId\":1149931888249675856,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Who dis\",\"SentAt\":\"2023-09-09T00:58:46.86-04:00\",\"Attachments\":[]},{\"MessageId\":1149931869144629258,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"\",\"SentAt\":\"2023-09-09T00:58:42.305-04:00\",\"Attachments\":[]},{\"MessageId\":1149931676575727616,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"\",\"SentAt\":\"2023-09-09T00:57:56.393-04:00\",\"Attachments\":[]},{\"MessageId\":1097224595762843748,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"\",\"SentAt\":\"2023-04-16T14:18:49.077-04:00\",\"Attachments\":[]},{\"MessageId\":1097223420355297321,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"\",\"SentAt\":\"2023-04-16T14:14:08.838-04:00\",\"Attachments\":[]},{\"MessageId\":1097216969922580690,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"\",\"SentAt\":\"2023-04-16T13:48:30.935-04:00\",\"Attachments\":[]},{\"MessageId\":1084371309640818728,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"love you too\",\"SentAt\":\"2023-03-12T03:04:26.863-04:00\",\"Attachments\":[]},{\"MessageId\":1084371283191529472,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Night love you\",\"SentAt\":\"2023-03-12T03:04:20.557-04:00\",\"Attachments\":[]},{\"MessageId\":1084371268259807232,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Thanks for letting me creep\",\"SentAt\":\"2023-03-12T03:04:16.997-04:00\",\"Attachments\":[]},{\"MessageId\":1084371136793546793,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/cry-gif-25866484\",\"SentAt\":\"2023-03-12T03:03:45.653-04:00\",\"Attachments\":[]},{\"MessageId\":1084371097203527740,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Guess I better go to bed now \U0001f979\",\"SentAt\":\"2023-03-12T03:03:36.214-04:00\",\"Attachments\":[]},{\"MessageId\":1084371012059148308,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"I kept forgetting\",\"SentAt\":\"2023-03-12T03:03:15.914-04:00\",\"Attachments\":[]},{\"MessageId\":1084371000449302549,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Yeah I'm annoyed\",\"SentAt\":\"2023-03-12T03:03:13.146-04:00\",\"Attachments\":[]},{\"MessageId\":1084370965506560001,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"shit bout to fuck up my whole week\",\"SentAt\":\"2023-03-12T03:03:04.815-04:00\",\"Attachments\":[]},{\"MessageId\":1084370906115211265,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"lmao it'll be 3AM pie now\",\"SentAt\":\"2023-03-12T03:02:50.655-04:00\",\"Attachments\":[]},{\"MessageId\":1084370830181552208,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"I was bout to go get me some pie 😭\",\"SentAt\":\"2023-03-12T03:02:32.551-04:00\",\"Attachments\":[]},{\"MessageId\":1084370788796350465,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"literally\",\"SentAt\":\"2023-03-12T03:02:22.684-04:00\",\"Attachments\":[]},{\"MessageId\":1084370744689049672,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"I was just tryna enjoy my weekend\",\"SentAt\":\"2023-03-12T03:02:12.168-04:00\",\"Attachments\":[]},{\"MessageId\":1084370734748532806,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"😭\",\"SentAt\":\"2023-03-12T03:02:09.798-04:00\",\"Attachments\":[]},{\"MessageId\":1084370714687180891,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"come on dawggggg\",\"SentAt\":\"2023-03-12T03:02:05.015-04:00\",\"Attachments\":[]},{\"MessageId\":1084370689286475870,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"daylight savings bitch ass\",\"SentAt\":\"2023-03-12T03:01:58.959-04:00\",\"Attachments\":[]},{\"MessageId\":1084370656134696970,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"WTF\",\"SentAt\":\"2023-03-12T03:01:51.055-04:00\",\"Attachments\":[]},{\"MessageId\":1084370636115288104,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"lol I gotta wait for the auto sorter thing to finish then I can go sell the rest of the shit\",\"SentAt\":\"2023-03-12T03:01:46.282-04:00\",\"Attachments\":[]},{\"MessageId\":1084370619572944927,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Ew it just turned 3am\",\"SentAt\":\"2023-03-12T03:01:42.338-04:00\",\"Attachments\":[]},{\"MessageId\":1084370480682778684,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Mans is over-encumbered\",\"SentAt\":\"2023-03-12T03:01:09.224-04:00\",\"Attachments\":[]},{\"MessageId\":1084368329206149281,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/you-did-it-willy-wonka-and-the-chocolate-factory-you-made-it-great-job-well-done-gif-21443346\",\"SentAt\":\"2023-03-12T01:52:36.272-05:00\",\"Attachments\":[]},{\"MessageId\":1084367706582700052,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/jumping-out-of-window-suicide-so-done-gif-8423077\",\"SentAt\":\"2023-03-12T01:50:07.827-05:00\",\"Attachments\":[]},{\"MessageId\":1084367143124090961,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/annoyed-fuck-frustrated-gif-15331740\",\"SentAt\":\"2023-03-12T01:47:53.488-05:00\",\"Attachments\":[]},{\"MessageId\":1084366802303336488,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/30rock-alec-baldwin-there-there-cheer-up-comfort-gif-4215371\",\"SentAt\":\"2023-03-12T01:46:32.23-05:00\",\"Attachments\":[]},{\"MessageId\":1084366692680994897,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/rage-angry-gif-25976706\",\"SentAt\":\"2023-03-12T01:46:06.094-05:00\",\"Attachments\":[]},{\"MessageId\":1084362194440966224,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/the-office-steve-carell-michael-scott-thats-what-she-said-gif-20446601\",\"SentAt\":\"2023-03-12T01:28:13.63-05:00\",\"Attachments\":[]},{\"MessageId\":1084362114539458602,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"The fact you used that gif is the best irony lmao\",\"SentAt\":\"2023-03-12T01:27:54.58-05:00\",\"Attachments\":[]},{\"MessageId\":1084362023724396575,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/facepalm-really-stressed-mad-angry-gif-16109475\",\"SentAt\":\"2023-03-12T01:27:32.928-05:00\",\"Attachments\":[]},{\"MessageId\":1084362000664121426,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Re:shards\",\"SentAt\":\"2023-03-12T01:27:27.43-05:00\",\"Attachments\":[]},{\"MessageId\":1084361957433425940,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"that's what she said\",\"SentAt\":\"2023-03-12T01:27:17.123-05:00\",\"Attachments\":[]},{\"MessageId\":1084360886438858763,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"mans had 14 gold\",\"SentAt\":\"2023-03-12T01:23:01.778-05:00\",\"Attachments\":[]},{\"MessageId\":1084358623829950494,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"well I was going the wrong way anyway, this is what I was looking for\",\"SentAt\":\"2023-03-12T01:14:02.33-05:00\",\"Attachments\":[]},{\"MessageId\":1084358508960555019,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"😂 😂\",\"SentAt\":\"2023-03-12T01:13:34.943-05:00\",\"Attachments\":[]},{\"MessageId\":1084358297366310972,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/ryan-reynolds-goddammit-damn-hitmans-bodyguard-hitmans-bodyguard-gifs-gif-8352668\",\"SentAt\":\"2023-03-12T01:12:44.495-05:00\",\"Attachments\":[]},{\"MessageId\":1084355935809585212,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Ouch\",\"SentAt\":\"2023-03-12T01:03:21.456-05:00\",\"Attachments\":[]},{\"MessageId\":1084355840628228096,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"will crash at least once per play\",\"SentAt\":\"2023-03-12T01:02:58.763-05:00\",\"Attachments\":[]},{\"MessageId\":1084355815139442759,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"yeah this modpack is unstable though, so gotta save a lot\",\"SentAt\":\"2023-03-12T01:02:52.686-05:00\",\"Attachments\":[]},{\"MessageId\":1084355628455165972,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"I gotta hop back on this game already\",\"SentAt\":\"2023-03-12T01:02:08.177-05:00\",\"Attachments\":[]},{\"MessageId\":1084355575162339338,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Not remembering to save will ruin your day fr\",\"SentAt\":\"2023-03-12T01:01:55.471-05:00\",\"Attachments\":[]},{\"MessageId\":1084354723106263100,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"save often\",\"SentAt\":\"2023-03-12T00:58:32.325-05:00\",\"Attachments\":[]},{\"MessageId\":1084354570207100989,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"now you see why I got 540 saves\",\"SentAt\":\"2023-03-12T00:57:55.871-05:00\",\"Attachments\":[]},{\"MessageId\":1084351461758095360,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"niggas broke\",\"SentAt\":\"2023-03-12T00:45:34.759-05:00\",\"Attachments\":[]},{\"MessageId\":1084348189051731968,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Probably for a lil bit longer\",\"SentAt\":\"2023-03-12T00:32:34.485-05:00\",\"Attachments\":[]},{\"MessageId\":1084348160215896084,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Si\",\"SentAt\":\"2023-03-12T00:32:27.61-05:00\",\"Attachments\":[]},{\"MessageId\":1084348132499918909,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"oh mkay\",\"SentAt\":\"2023-03-12T00:32:21.002-05:00\",\"Attachments\":[]},{\"MessageId\":1084348109225730099,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"didn't know you were still watching\",\"SentAt\":\"2023-03-12T00:32:15.453-05:00\",\"Attachments\":[]},{\"MessageId\":1084348057266700299,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"I was googling what that thing did lol\",\"SentAt\":\"2023-03-12T00:32:03.065-05:00\",\"Attachments\":[]},{\"MessageId\":1084348013692080218,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"nvm\",\"SentAt\":\"2023-03-12T00:31:52.676-05:00\",\"Attachments\":[]},{\"MessageId\":1084347992506650696,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Intermission?\",\"SentAt\":\"2023-03-12T00:31:47.625-05:00\",\"Attachments\":[]},{\"MessageId\":1084339613499400253,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"you should probably let them finish their dialogue first\",\"SentAt\":\"2023-03-11T23:58:29.914-05:00\",\"Attachments\":[]},{\"MessageId\":1084339535313383434,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"lmaooo\",\"SentAt\":\"2023-03-11T23:58:11.273-05:00\",\"Attachments\":[]},{\"MessageId\":1084339528069812334,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"sir\",\"SentAt\":\"2023-03-11T23:58:09.546-05:00\",\"Attachments\":[]},{\"MessageId\":1084339458612133908,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"time to google who tf alain dufont is\",\"SentAt\":\"2023-03-11T23:57:52.986-05:00\",\"Attachments\":[]},{\"MessageId\":1084339391696212048,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"damn hope he wasn't important\",\"SentAt\":\"2023-03-11T23:57:37.032-05:00\",\"Attachments\":[]},{\"MessageId\":1084338558896189450,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Katara took  mans out\",\"SentAt\":\"2023-03-11T23:54:18.477-05:00\",\"Attachments\":[]},{\"MessageId\":1084336903400198164,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Jafar and Jasmine baby\",\"SentAt\":\"2023-03-11T23:47:43.776-05:00\",\"Attachments\":[]},{\"MessageId\":1084336501590073395,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Why mans got the Princess Jasmine ponytail\",\"SentAt\":\"2023-03-11T23:46:07.977-05:00\",\"Attachments\":[]},{\"MessageId\":1084335115473268736,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"wtf\",\"SentAt\":\"2023-03-11T23:40:37.501-05:00\",\"Attachments\":[]},{\"MessageId\":1084333958155739146,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/j-alexander-antm-americas-next-top-model-clutches-pearls-clutch-neck-gif-21070072\",\"SentAt\":\"2023-03-11T23:36:01.575-05:00\",\"Attachments\":[]},{\"MessageId\":1084333849179345026,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"i know what else is heavy\",\"SentAt\":\"2023-03-11T23:35:35.593-05:00\",\"Attachments\":[]},{\"MessageId\":1084333265546137642,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"lmaoo\",\"SentAt\":\"2023-03-11T23:33:16.444-05:00\",\"Attachments\":[]},{\"MessageId\":1084333231274463323,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/fruit-dancing-food-gif-4808470\",\"SentAt\":\"2023-03-11T23:33:08.273-05:00\",\"Attachments\":[]},{\"MessageId\":1084332417520779325,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"She out here living lezzy life\",\"SentAt\":\"2023-03-11T23:29:54.259-05:00\",\"Attachments\":[]},{\"MessageId\":1084332186301386762,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"I got a bunch of women living with me\",\"SentAt\":\"2023-03-11T23:28:59.132-05:00\",\"Attachments\":[]},{\"MessageId\":1084331457306181632,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"Si\",\"SentAt\":\"2023-03-11T23:26:05.326-05:00\",\"Attachments\":[]},{\"MessageId\":1084331420157227039,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"can you see\",\"SentAt\":\"2023-03-11T23:25:56.469-05:00\",\"Attachments\":[]},{\"MessageId\":1084331193790627850,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"https://tenor.com/view/forest-gump-wave-hi-hello-howdy-gif-22164679\",\"SentAt\":\"2023-03-11T23:25:02.499-05:00\",\"Attachments\":[]},{\"MessageId\":1084331085179146301,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"\",\"SentAt\":\"2023-03-11T23:24:36.604-05:00\",\"Attachments\":[]},{\"MessageId\":1084331038014193765,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1084330569141329932,\"ChannelName\":\"general\",\"MessageContent\":\"\",\"SentAt\":\"2023-03-11T23:24:25.359-05:00\",\"Attachments\":[]},{\"MessageId\":1149932632033988671,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1149932420406198312,\"ChannelName\":\"channel-1\",\"MessageContent\":\"\",\"SentAt\":\"2023-09-09T01:01:44.192-04:00\",\"Attachments\":[{\"AttachmentType\":\"image/jpeg\",\"DownloadUrl\":\"https://media.discordapp.net/attachments/1149932420406198312/1149932631824289832/IMG_7841.png\"}]},{\"MessageId\":1149932751588438027,\"GuildId\":1084330568667377736,\"GuildName\":\"JayNik\",\"ChannelId\":1149932444959658064,\"ChannelName\":\"channel-2\",\"MessageContent\":\"\",\"SentAt\":\"2023-09-09T01:02:12.696-04:00\",\"Attachments\":[{\"AttachmentType\":\"image/jpeg\",\"DownloadUrl\":\"https://media.discordapp.net/attachments/1149932444959658064/1149932751361941546/6D6BAFB0-1666-424D-AD60-698BE39895C9.png\"}]}]";

        #endregion

        #region Public API

        public async override void Run()
        {
            await Configure();
            await RunProgramLoop();
        }

        #endregion

        #region Private API: User Inferface

        protected override List<ListOption> GetListOptions()
        {
            List<ListOption> listOptions = new List<ListOption>();

            listOptions.Add(new ListOption("Show Holders", ShowHolders));
            listOptions.Add(new ListOption("Manage Excluded Users", ManageExcludedUsers));
            listOptions.Add(new ListOption("Purge Invalid Users", PurgeInvalidUsers));
            listOptions.Add(new ListOption("Run Scheduled User Purge", RunScheduledPurge));
            listOptions.Add(new ListOption("Synchronize Data", SynchronizeData));
            listOptions.Add(new ListOption("Run Scheduled Synchronization", RunScheduledSync));
            listOptions.Add(new ListOption("Stop Sync Routines", StopSyncRoutines));
            listOptions.Add(new ListOption("Ensure Channel Permissions", EnsureChannelPermissions));
            listOptions.Add(new ListOption("Edit Channel Of The Day", EditChannelOfTheDay));
            listOptions.Add(new ListOption("Edit Configuration", EditConfiguration));
            listOptions.Add(new ListOption("Manage Target Servers", EditTargetServerConfig));
            listOptions.Add(new ListOption("Manage Excluded Channels", EditChannelConfig));
            listOptions.Add(new ListOption("Manage Roles", EditRoleConfig));
            listOptions.AddRange(base.GetListOptions());
            listOptions.Add(GetHelpOption());

            return listOptions;
        }

        #endregion

        #region Private API: Configuration

        private async Task Configure()
        {
            XMLSettings.GetSetting(Setting.WalletAddress);
            XMLSettings.GetSetting(Setting.GoogleSheetId);
            XMLSettings.GetSetting(ConnectionStrings.Discord);

            if (Client == null)
            {
                Client = new FriendBotClient();
                Client.DiscordServerId = ulong.Parse(XMLSettings.GetSetting(Setting.DiscordServerId, ValidateSetting));
                Client.BotToken = XMLSettings.GetSetting(Setting.DiscordBotToken, null, new PromptSettings() { IsSecret = true });
            }

            if (ExtractionClient == null)
            {
                ExtractionClient = new DataExtractionClient();
                ExtractionClient.DiscordUserToken = XMLSettings.GetSetting(Setting.DiscordUserToken, null, new PromptSettings() { IsSecret = true });
            }

            if (!Client.Initialized)
            {
                try { await Client.Initialize(); }
                catch (Exception e) { e.LogException(); }
            }

            if (!ExtractionClient.Initialized)
            {
                try { await ExtractionClient.Initialize(); }
                catch (Exception e) { e.LogException(); }
            }

            while (!Client.Initialized || !ExtractionClient.Initialized)
            { // wait
            }

            Thread.Sleep(1000);

            string extractionServerIds = XMLSettings.GetValue(Setting.DiscordServerExtractionSettings);

            if (string.IsNullOrEmpty(extractionServerIds) || !ValidateSetting(Setting.DiscordServerExtractionSettings, extractionServerIds))
                EditTargetServerConfig();

            ulong.TryParse(XMLSettings.GetSetting(Setting.DiscordLogChannelId, ValidateSetting), out ulong logChannelId);
            Client.LogChannelId = logChannelId;

            await Task.CompletedTask;
        }

        private void EditConfiguration()
        {
            bool proceed = Utilities.GetConfirmation("Are you sure you want to edit the main configuration?  Existing configuration settings will be lost, and all bot execution will be stopped until configuration is re-applied.");

            if (!proceed)
                return;

            StopSyncRoutines();

            Client.Shutdown();
            ExtractionClient.Shutdown();

            Client = null;
            ExtractionClient = null;

            XMLSettings.Update(Setting.DiscordServerExtractionSettings, null);
            XMLSettings.Update(Setting.DiscordUserToken, null);
            XMLSettings.Update(Setting.DiscordBotToken, null);
            XMLSettings.Update(Setting.DiscordServerId, null);
            XMLSettings.Update(Setting.DiscordLogChannelId, null);

            Configure();
        }

        private void EditTargetServerConfig()
        {
            List<DiscordGuildExtractionConfig> extractionSettings = DiscordManager.GetExtractionSettings();
            List<DiscordGuildDTO> userGuilds = ExtractionClient.GetAvailableGuilds();

            MultiSelectionPrompt<DiscordGuildDTO> prompt = new MultiSelectionPrompt<DiscordGuildDTO>();
            prompt.Title = Setting.DiscordServerExtractionSettings.GetPrompt();
            prompt.InstructionsText = "[grey](Press [blue]<space>[/] to toggle an option, [green]<enter>[/] to continue)[/]\n";
            prompt.Required = false;
            prompt.PageSize = 20;
            prompt.UseConverter(x => string.Format("{0} ({1})", x.GuildName, x.GuildId));

            userGuilds.ForEach(x =>
            {
                IMultiSelectionItem<DiscordGuildDTO> item = prompt.AddChoice(x);

                if (extractionSettings.Any(y => y.GuildId == x.GuildId))
                    item.Select();
            });

            List<DiscordGuildDTO> choices = AnsiConsole.Prompt(prompt);

            choices.ForEach(x =>
            {
                if (!extractionSettings.Any(y => y.GuildId == x.GuildId))
                    extractionSettings.Add(new DiscordGuildExtractionConfig() { Name = x.GuildName, GuildId = x.GuildId });
            });

            extractionSettings = extractionSettings.Where(x => choices.Any(y => y.GuildId == x.GuildId)).ToList();

            XMLSettings.Update(Setting.DiscordServerExtractionSettings, JsonConvert.SerializeObject(extractionSettings));

            AnsiConsole.WriteLine("Target servers updated successfully!");
            AnsiConsole.WriteLine();
        }

        private void EditChannelConfig()
        {
            List<DiscordGuildExtractionConfig> extractionSettings = DiscordManager.GetExtractionSettings();

            SelectionPrompt<DiscordGuildExtractionConfig> extractionPrompt = new SelectionPrompt<DiscordGuildExtractionConfig>();
            extractionPrompt.Title = "Select the server whose channels you want to manage";
            extractionPrompt.PageSize = 20;
            extractionPrompt.UseConverter(x => string.Format("{0} ({1})", x.Name, x.GuildId));
            extractionPrompt.AddChoices(extractionSettings);

            DiscordGuildExtractionConfig setting = AnsiConsole.Prompt(extractionPrompt);   
            extractionSettings.Remove(setting);

            List<DiscordChannelDTO> availableChannels = ExtractionClient.GetAvailableChannels()
                                                        .Where(x => x.GuildId == setting.GuildId)
                                                        .ToList();

            MultiSelectionPrompt<DiscordChannelDTO> prompt = new MultiSelectionPrompt<DiscordChannelDTO>();
            prompt.Title = "Select the channels you want to exclude from data extraction";
            prompt.InstructionsText = "[grey](Press [blue]<space>[/] to toggle an option, [green]<enter>[/] to continue)[/]\n";
            prompt.Required = false;
            prompt.PageSize = 20;
            prompt.UseConverter(x => string.Format("{0} ({1})", x.ChannelName, x.ChannelId));


            availableChannels.Where(x => !x.ParentChannelId.HasValue)
            .ToList()
            .ForEach(x =>
            {
                List<DiscordChannelDTO> children = availableChannels.Where(y => y.ParentChannelId == x.ChannelId).ToList();
                prompt.AddChoiceGroup(x, children);

                if (setting.ExcludedChannelIds != null) 
                {
                    if(setting.ExcludedChannelIds.Any(y => y == x.ChannelId))
                        prompt.Select(x);

                    children.ForEach(child =>
                    {
                        if (setting.ExcludedChannelIds.Any(y => y == child.ChannelId))
                            prompt.Select(child);
                    });
                } 
            });

            List<DiscordChannelDTO> choices = AnsiConsole.Prompt(prompt);
            List<ulong> excludedChannelIds = choices.Select(y => y.ChannelId).ToList();

            availableChannels.Where(x => x.ParentChannelId.HasValue)
            .GroupBy(x => x.ParentChannelId.Value)
            .ToList()
            .ForEach(group =>
            {
                if (group.All(x => excludedChannelIds.Contains(x.ChannelId)))
                    excludedChannelIds.Add(group.Key);
            });

            setting.ExcludedChannelIds = excludedChannelIds;
            extractionSettings.Add(setting);

            XMLSettings.Update(Setting.DiscordServerExtractionSettings, JsonConvert.SerializeObject(extractionSettings));

            AnsiConsole.WriteLine("Target server channels updated successfully!");
            AnsiConsole.WriteLine();
        }

        private void EditRoleConfig()
        {
            List<DiscordRoleConfig> currentConfigs = GetRoleConfigurations();

            List<SocketRole> roles = Client.GetRoles();

            SelectionPrompt<SocketRole> prompt = new SelectionPrompt<SocketRole>();
            prompt.Title = Setting.DiscordRoleConfiguration.GetPrompt();
            prompt.PageSize = 20;
            prompt.UseConverter(x => string.Format("{0} ({1})", x.Name, x.Id));
            prompt.AddChoices(roles);

            SocketRole choice = AnsiConsole.Prompt(prompt);

            string keys = Utilities.GetInput("How many keys should a user hold for this role?", x => x.All(y => char.IsDigit(y)));
            int.TryParse(keys, out int numKeys);

            DiscordRoleConfig config = currentConfigs.FirstOrDefault(x => x.RoleId == choice.Id);

            if (config == null)
            {
                config = new DiscordRoleConfig();
                config.RoleId = choice.Id;
                config.RoleName = choice.Name;
                currentConfigs.Add(config);
            }

            config.NumKeys = numKeys;
            XMLSettings.Update(Setting.DiscordRoleConfiguration, JsonConvert.SerializeObject(currentConfigs));

            AnsiConsole.MarkupLine("[green]The role '{0}' has been updated successfully![/]", choice.Name);
        }

        private bool ValidateSetting(Setting node, string value)
        {
            bool validated = true;

            if (node.Name == Setting.DiscordServerId.Name || node.Name == Setting.DiscordLogChannelId.Name)
            {
                validated = value.All(x => char.IsDigit(x));

                if (!validated)
                    Console.WriteLine("Invalid server Id.");
            }
            else if (node.Name == Setting.DiscordServerExtractionSettings.Name)
            {
                try
                {
                    JsonConvert.DeserializeObject<List<DiscordGuildExtractionConfig>>(value);
                }
                catch (Exception)
                {
                    validated = false;
                    Console.WriteLine("The current list of target server ids is invalid, please re-configure.");
                }
            }
            else if (node.Name == Setting.DiscordSyncInterval.Name || node.Name == Setting.DiscordPurgeInterval.Name)
            {
                int minInterval = node.Name == Setting.DiscordSyncInterval.Name ? Setting.MinSyncInterval : Setting.MinPurgeInterval;

                validated = value.All(x => char.IsDigit(x)) && int.Parse(value) >= minInterval;

                if (!validated)
                    Console.WriteLine("Invalid interval.");
            }

            return validated;
        }

        #endregion

        #region Private API: User Management

        private async void PurgeInvalidUsers()
        {
            await RunPurgeRoutine();
        }

        private async void RunScheduledPurge()
        {
            ScheduledPurgeEnabled = true;

            int.TryParse(XMLSettings.GetSetting(Setting.DiscordPurgeInterval, ValidateSetting), out int interval);

            if (interval == 0 || interval < Setting.MinPurgeInterval)
            {
                AnsiConsole.WriteLine("Invalid purge interval found in configuration settings, using default value of {0} minutes", Setting.DefaultPurgeInterval);
                interval = Setting.DefaultPurgeInterval;
            }

            interval = (int)TimeSpan.FromMinutes(interval).TotalMilliseconds;

            while (ScheduledPurgeEnabled && Client != null && ExtractionClient != null && Client.Initialized && ExtractionClient.Initialized)
            {
                await RunPurgeRoutine();
                await Task.Delay(interval);
            }
        }

        private async Task RunPurgeRoutine()
        {
            try
            {
                RunningPurgeRoutine = true;

                AnsiConsole.MarkupLine("\n[yellow]Invalid User Purge In Progress - {0}[/]", DateTime.Now.ToShortTimeString());

                await Client.PurgeInvalidUsers();

                AnsiConsole.MarkupLine("[green]Invalid User Purge Complete - {0}[/]\n", DateTime.Now.ToShortTimeString());

                RunningPurgeRoutine = false;

            }
            catch (Exception e)
            {
                e.LogException();
                RunningPurgeRoutine = false;
            }

            await Task.CompletedTask;
        }

        private async void EnsureChannelPermissions()
        {
            if (RunningSyncRoutine || RunningPurgeRoutine)
            {
                AnsiConsole.MarkupLine("\n[orange1]Ensuring Channel Permissions - Waiting for other routines to complete... {0}[/]", DateTime.Now.ToShortTimeString());

                while (RunningSyncRoutine || RunningPurgeRoutine)
                    await Task.Delay(2000);
            }

            try
            {
                AnsiConsole.MarkupLine("\n[yellow]Ensuring Channel Permissions - {0}[/]", DateTime.Now.ToShortTimeString());

                RunningChannelPermissionsRoutine = true;
                List<DiscordChannelModel> channels = await GetChannels();

                await Client.EnsureChannelPermissions(channels);

                AnsiConsole.MarkupLine("[green]Ensuring Channel Permissions Complete - {0}[/]\n", DateTime.Now.ToShortTimeString());

                RunningChannelPermissionsRoutine = false;

            }
            catch (Exception e)
            {
                e.LogException();
                RunningChannelPermissionsRoutine = false;
            }

            await Task.CompletedTask;
        }

        private void EditChannelOfTheDay()
        {
            if (RunningSyncRoutine || RunningPurgeRoutine)
            {
                AnsiConsole.MarkupLine("\nWaiting for other routines to complete... {0}[/]", DateTime.Now.ToShortTimeString());

                while (RunningSyncRoutine || RunningPurgeRoutine)
                    Thread.Sleep(2000);
            }

            List<DiscordChannelModel> channels = Task.Run(() => GetChannels()).Result;

            if (channels == null || !channels.Any())
            {
                AnsiConsole.MarkupLine("[red]No channels found![/]");
                return;
            }

            SelectionPrompt<DiscordChannelModel> prompt = new SelectionPrompt<DiscordChannelModel>();
            prompt.Title = "Select the channel you want to add as channel of the day";
            prompt.PageSize = 20;
            prompt.UseConverter(x => string.Format("{0} ({1})", x.Name, x.Id));
            prompt.AddChoices(channels);
            prompt.AddChoice(new DiscordChannelModel { Name = "None", Id = 0 });

            DiscordChannelModel channel = AnsiConsole.Prompt(prompt);

            Client.ChannelOfTheDayId = channel.Id;

            EnsureChannelPermissions();
        }

        #endregion

        #region Private API: Synchronization

        private async void SynchronizeData()
        {
            await RunSyncRoutine();
        }

        private async void RunScheduledSync()
        {
            ScheduledSyncEnabled = true;
            int.TryParse(XMLSettings.GetSetting(Setting.DiscordSyncInterval, ValidateSetting), out int syncInterval);

            if (syncInterval == 0 || syncInterval < Setting.MinSyncInterval)
            {
                AnsiConsole.WriteLine("Invalid sync interval found in configuration settings, using default value of {0} minutes", Setting.DefaultSyncInterval);
                syncInterval = Setting.DefaultSyncInterval;
            }

            syncInterval = (int)TimeSpan.FromMinutes(syncInterval).TotalMilliseconds;

            while (ScheduledSyncEnabled && Client != null && ExtractionClient != null && Client.Initialized && ExtractionClient.Initialized)
            {
                await RunSyncRoutine();
                await Task.Delay(syncInterval);
            }
        }

        private async Task RunSyncRoutine()
        {
            try
            {
                RunningSyncRoutine = true;

                AnsiConsole.MarkupLine("\n[yellow]Synchronization In Progress - {0}[/]", DateTime.Now.ToShortTimeString());

                List<DiscordChannelModel> channels = await GetChannels();

                List<DiscordMessageDTO> messages = await ExtractionClient.DownloadGuildData(channels); //JsonConvert.DeserializeObject<List<DiscordMessageDTO>>(Messages); 
                await Client.SynchronizeChannels(channels, messages);

                AnsiConsole.MarkupLine("[green]Synchronization Complete - {0}[/]\n", DateTime.Now.ToShortTimeString());

                RunningSyncRoutine = false;

            }
            catch (Exception e)
            {
                e.LogException();
                RunningSyncRoutine = false;
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Private API

        public static List<DiscordGuildExtractionConfig> GetExtractionSettings()
        {
            string val = XMLSettings.GetValue(Setting.DiscordServerExtractionSettings);
            List<DiscordGuildExtractionConfig> settings = !string.IsNullOrEmpty(val) ? JsonConvert.DeserializeObject<List<DiscordGuildExtractionConfig>>(val) : new List<DiscordGuildExtractionConfig>();
            return settings;
        }

        public static List<DiscordRoleConfig> GetRoleConfigurations()
        {

            string currentConfig = XMLSettings.GetValue(Setting.DiscordRoleConfiguration);
            List<DiscordRoleConfig> currentConfigs = new List<DiscordRoleConfig>();

            if (!string.IsNullOrEmpty(currentConfig))
                currentConfigs = JsonConvert.DeserializeObject<List<DiscordRoleConfig>>(currentConfig);

            return currentConfigs;
        }

        public static async Task<List<DiscordChannelModel>> GetChannels()
        {
            List<ulong> targetServerIds = DiscordManager.GetExtractionSettings().Select(x => x.GuildId).ToList();

            List<DiscordChannelModel> channels = await DiscordChannelModel.GetAll(new() { x => targetServerIds.Contains(x.SourceGuildId) }, new() { x => x.Guild, x => x.Logs });

            return channels;
        }

        private void StopSyncRoutines()
        {
            Status status = AnsiConsole.Status()
                            .AutoRefresh(false)
                            .Spinner(Spinner.Known.Star)
                            .SpinnerStyle(Style.Parse("green bold"));

            if (RunningSyncRoutine || RunningPurgeRoutine)
            {
                AnsiConsole.Status()
                .Start("A scheduled routine is currently running, waiting for execution to complete...", ctx =>
                {
                    // Simulate some work
                    while (RunningSyncRoutine || RunningPurgeRoutine)
                        Thread.Sleep(1000);
                });
            }

            AnsiConsole.MarkupLine("[green]All synchronization routines have been stopped successfully[/]");

            ScheduledSyncEnabled = false;
            ScheduledPurgeEnabled = false;
        }


        #endregion
    }
}
