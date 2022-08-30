using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using TwitterSharp.Client;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using TwitterSharp.Response.RTweet;
using TwitterSharp.Response.RUser;

namespace Spoopy.Services
{
    public class TwitterService
    {
        private TwitterClient _twitterClient;

        public TwitterService()
        {
            var bearerToken = Environment.GetEnvironmentVariable("Bearer_Token_Spoopy", EnvironmentVariableTarget.User);
            _twitterClient = new TwitterClient(bearerToken);
        }

        public enum TwitterClientMethod
        {
            GetUser,
            GetTweet,
            GetTweets,
        }

        public async Task<EmbedBuilder> CreateTweetEmbedAsync(Tweet tweet, string title)
        {
            try
            {
                TimeSpan tweetosTime = tweet.CreatedAt.Value.TimeOfDay;
                int tweetosTimeHour = tweetosTime.Hours + await Utilities.GetParisTimeZoneAsync();
                TimeSpan tweetTime = new TimeSpan(tweetosTimeHour, tweetosTime.Minutes, tweetosTime.Seconds);

                var embedTweet = new EmbedBuilder();
                embedTweet.WithColor(Color.Blue)
                    .WithAuthor($"{tweet.Author.Name} @{tweet.Author.Username}")
                    .WithTitle($"{title} {Utilities.DeFactory(tweet.Author.Name)}{tweet.Author.Name}")
                    .WithUrl("https://twitter.com/" + tweet.Author.Username + $"/status/{tweet.Id}")
                    .WithThumbnailUrl(tweet.Author.ProfileImageUrl)
                    .WithDescription(Utilities.DeleteUrlFromText(tweet.Text))
                    .WithFooter($"Publié à {tweetTime.ToString(@"hh\:mm")} le {tweet.CreatedAt.Value.Date.ToString("dd MMMM, yyyy", Properties.Culture)} ", iconUrl: Properties.TwitterLogoURL)
                    .AddField("Replys : ", $"`{tweet.PublicMetrics.ReplyCount}`", inline: true)
                    .AddField("RTs : ", $"`{tweet.PublicMetrics.RetweetCount}`", inline: true)
                    .AddField("Likes : ", $"`{tweet.PublicMetrics.LikeCount}`", inline: true);
                
                if (tweet.Attachments != null)
                {
                    if (tweet.Attachments.Media[0].Url != null)
                        embedTweet.WithImageUrl(tweet.Attachments.Media[0].Url);
                    if (tweet.Attachments.Media[0].PreviewImageUrl != null)
                    {
                        embedTweet.WithImageUrl(tweet.Attachments.Media[0].PreviewImageUrl);
                        embedTweet.WithDescription($"{Utilities.DeleteUrlFromText(tweet.Text)} \n\n `Ce tweet contient une vidéo de {(tweet.Attachments.Media[0].DurationMs) / 1000} secondes`");
                    }

                }
                if (tweet.ReferencedTweets != null)
                {
                    if (tweet.ReferencedTweets.First().Type == ReferenceType.Quoted)
                    {
                        ReferencedTweet referencedTweet = tweet.ReferencedTweets.First();
                        Tweet quoteTweet = (Tweet)await UseTwitterClientAsync(method: TwitterClientMethod.GetTweet, id: referencedTweet.Id);
                        embedTweet.AddField($"__Cité de {quoteTweet.Author.Name} @{quoteTweet.Author.Username}__", Utilities.DeleteUrlFromText(quoteTweet.Text));
                        if (quoteTweet.Attachments != null && tweet.Attachments == null)
                        {
                            if (quoteTweet.Attachments.Media[0].Url != null)
                                embedTweet.WithImageUrl(quoteTweet.Attachments.Media[0].Url);
                            if (quoteTweet.Attachments.Media[0].PreviewImageUrl != null)
                                embedTweet.WithImageUrl(quoteTweet.Attachments.Media[0].PreviewImageUrl);
                        }
                    }
                }

                return embedTweet;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Program.ZLog(message: "Erreur dans la création d'embed Twitter", isError: true);
                return null;
            }
            
        }
          
        public async Task<object> UseTwitterClientAsync(TwitterClientMethod method, string id = null, string username = null)
        {           
            if (method == TwitterClientMethod.GetTweet || method == TwitterClientMethod.GetTweets)
            {
                Tweet tweet = await GetTweetWithCustomSearchAsync(method, id);
                return tweet;
            }
            else
            {
                User user = await GetTwitterUser(username);
                return user;
            }

        }

        public async Task<User> GetTwitterUser(string username)
        {
            User user = await _twitterClient.GetUserAsync(username);
            return user;
        }

        public async Task<Tweet> GetTweetWithCustomSearchAsync(TwitterClientMethod method, string id)
        {
            TweetOption[] everyTweetOptions = (TweetOption[])Enum.GetValues(typeof(TweetOption));
            TweetOption[] tweetOption = new TweetOption[]
            {
                everyTweetOptions[1], // Date de publication
                everyTweetOptions[10], // Attachments
                everyTweetOptions[3], // In Reply to Tweet
                everyTweetOptions[7], // Referenced Tweet
                everyTweetOptions[6], // RT, Likes, Quote, Reply Counts
            };

            UserOption[] everyUserOptions = (UserOption[])Enum.GetValues(typeof(UserOption));
            UserOption[] userOption = new UserOption[]
            {
                everyUserOptions[5], // Profile Image Url
                everyUserOptions[8], // Url
                everyUserOptions[6], // Protected
            };

            MediaOption[] everyMediaOptions = (MediaOption[])Enum.GetValues(typeof(MediaOption));
            MediaOption[] mediaOption = new MediaOption[]
            {
                everyMediaOptions[4],  // Image Preview Url
                everyMediaOptions[3],  // Url
                everyMediaOptions[0], // Duration
            };           

            Tweet tweet;
            var tweetOptions = new TweetSearchOptions();
            tweetOptions.TweetOptions = tweetOption;
            tweetOptions.UserOptions = userOption;
            tweetOptions.MediaOptions = mediaOption;

            if (method == TwitterClientMethod.GetTweet)
            {
                tweet = await _twitterClient.GetTweetAsync(id, options: tweetOptions);
            }
            else
            {
                Tweet[] tweets = await _twitterClient.GetTweetsFromUserIdAsync(id, options: tweetOptions);
                tweet = tweets.FirstOrDefault(tweetos => tweetos.ReferencedTweets == null || tweetos.ReferencedTweets.First().Type == ReferenceType.Quoted);
            }
            return tweet;
        }

    }
}
