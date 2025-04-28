using DSharpPlus.Entities;
using FalconsRoost.Models.Alerts;
using FalconsRoost.Models.db;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Policy;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FalconsRoost.WebScrapers
{
    public interface IShopifyAlert
    {
        //public Task<bool> WatchPageForSale(AlertTask task);
        public Task<List<DiscordEmbed>> SearchForComic(string searchTerm, ulong userId, ulong channelId);
        public Task ProcessThirdEyeMonitors();
    }

    public class ThirdEyeScraper : BaseScraper, IShopifyAlert
    {
        private string _searchUrl = "https://shop.thirdeyecomics.com/search";
        private string _baseImageUrl = "https://shop.thirdeyecomics.com/cdn/shop/";
        private string _productDetailUrl = "https://shop.thirdeyecomics.com/apps/reviews/products?url=";

        public ThirdEyeScraper(IConfiguration config, FalconsRoostDBContext context) : base(config, context)
        {
        }

        public async Task<List<DiscordEmbed>> SearchForComic(string searchTerm, ulong channelId, ulong userId)
        {
            //so, the search term could come in like "I am a query" or "taco" - we want to replace spaces with +
            //and make sure we don't have any weird characters.
            var searchUrl = _searchUrl + "?q=" + UrlEncoder.Default.Encode(searchTerm) + "&type=products&view=samitaLabelsProductsJson";
            var payloadJson = await GetJSON(searchUrl);
            var success = !string.IsNullOrWhiteSpace(payloadJson);
            var embeds = new List<DiscordEmbed>();
            if (success)
            {
                //their json is broken, so we need to fix it.
                var fixedJson = FixThirdEyeJson(payloadJson);
                List<ThirdEyeSearchProducts> searchPayload = null;
                try
                {
                    searchPayload = JsonSerializer.Deserialize<List<ThirdEyeSearchProducts>>(fixedJson);
                }
                catch(Exception e)
                {
                    //if we get an error, we want to know about it.
                    var message = $"There was an error getting the page. Did we get blocked? URL: {searchUrl} - EXCEPTION: {e.Message}";
                    await SendMessage(new AlertMessage(), message);
                    return embeds;
                }
                
                if (searchPayload != null)
                {
                    //now we need to remove garbage data.
                    searchPayload = searchPayload.Where(c => c.available != null).ToList();
                    if (!searchPayload.Any())
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Title = "No products found.",
                            Color = DiscordColor.Red,
                        }.Build();
                        embeds.Add(embed);
                        return embeds;
                    }

                    //we only want to send the top 5.
                    searchPayload = searchPayload.OrderByDescending(c => c.created_at).Take(5).ToList();
                    foreach (var product in searchPayload)
                    {
                        var guid = Guid.NewGuid();
                        var title = product.title;
                        var incentive = product.tags.Any(c => c.Contains("incentive")) ? " - Incentive Cover - " : "";
                        var available = product.available == true ? $"Available" : "Not Available";

                        //if productPrice is set, it is a string with a number and no decimal. We want to convert that to $xx.xx
                        string formattedPrice = "Unknown Price";
                        if (!string.IsNullOrWhiteSpace(product.price))
                        {
                            var price = decimal.Parse(product.price) / 100;
                            formattedPrice = string.Format("{0:C}", price);
                        }
                        else if (product.variants != null && product.variants.Any())
                        {
                            var vPrice = product.variants?.FirstOrDefault()?.price ?? 0;
                            var variantPrice = (decimal)(vPrice / 100);
                            formattedPrice = string.Format("{0:C}", variantPrice);
                        }
                        else
                        {
                            formattedPrice = "Unknown Price";
                        }
                         
                        var description = $"{formattedPrice}{incentive} - {available}";
                        var handle = product.handle;
                        var imgUrl = _baseImageUrl + product.featured_image ?? string.Empty;

                        try
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Title = title,
                                Color = DiscordColor.Green,
                            }
                            .WithUrl($"https://shop.thirdeyecomics.com/products/{handle}")
                            .WithImageUrl(imgUrl)
                            .WithDescription(description)
                            .WithFooter($"Register for an alert with !3eAlert {guid}")
                            .Build();
                            embeds.Add(embed);

                            //we also want to register this in the alert cache.
                            TempAlertCache.Register(guid.ToString(), handle, title, channelId, userId);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error creating embed for product {product.id}: {ex.Message}", ex);
                        }
                    }
                }
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Failed to get information from Third Eye.",
                    Color = DiscordColor.Red,
                }.Build();
                embeds.Add(embed);
            }
            return embeds;

        }

        public async Task ProcessThirdEyeMonitors()
        {
            var tasks = TempAlertCache.GetActiveRecords();
            
            //we only want to check each handle once, but may need to trigger multiple notifications.
            var handles = tasks.Select(c => c.Handle).Distinct().ToList();

            if(!handles.Any())
            {
                //we don't have any handles to check, so we can bail.
                return;
            }

            var random = new Random();
            foreach (var handle in handles)
            {
                //we want to wait a random amount of time between 1 and 5 seconds before checking each handle.
                var sleepTime = random.Next(1000, 5000);
                await Task.Delay(sleepTime);
                var embeds = await CheckSpecificComicAvailability(handle);
                if (embeds.Any())
                {
                    //we need to get the channel id for each task that matches this handle.
                    var notifyTargets = tasks.Where(c => c.Handle == handle);
                    foreach(var target in notifyTargets)
                    {
                        await SendMessage(embeds.First(), target.ChannelId);
                        //we want to remove the alert from the cache.
                        TempAlertCache.Remove(target.Id);
                    }
                }
            }
        }

        public async Task<bool> WatchPageForSale(AlertTask task)
        {
            throw new NotImplementedException("This method is not implemented yet.");

            string targetUrl = "www.google.com"; 
            var targetText = "Add to cart";

            if (string.IsNullOrEmpty(targetUrl) || string.IsNullOrEmpty(targetText))
            {
                //we're not correctly configured. Burn it!
                var message = $"ThirdEyeComicsScraper is not configured correctly. TargetUrl: {targetUrl} TargetText: {targetText}";
                await SendMessage(task.AlertMessages.First(), message);
                task.Enabled = false;
                task.CurrentlyRunning = false;
                await UpdateTask(task);
                return false;
            }
            task.CurrentlyRunning = true;
            await UpdateTask(task);
            var stillRunning = true;
            try
            {
                while (stillRunning)
                {
                    var page = GetHtml(targetUrl);

                    //we want to get the node with the id "AddToCart"
                    var cartButton = page.SelectSingleNode("//button[@id='AddToCart']");
                    var isDisabled = cartButton?.Attributes.Contains("disabled") ?? false;

                    if (!isDisabled)
                    {
                        //The page says we can add to cart. GOGO!
                        var message = $"Found target text: {targetText} on page: {targetUrl}";
                        await SendMessage(task.AlertMessages.First(), message);
                        task.CurrentlyRunning = false;
                        task.Enabled = false;
                        await UpdateTask(task);
                        stillRunning = false;
                        return true;
                    }
                    else
                    {
                        //we didn't find the target text, so we can sleep for a bit.
                        var sleepTime = new Random().Next(1500, 10000);
                        await Task.Delay(sleepTime);
                    }
                }
            }
            catch (Exception ex)
            {
                //if we get an error, we want to know about it.
                var message = $"There was an error getting the page. Did we get blocked? URL: {targetUrl} - EXCEPTION: {ex.Message}";
                await SendMessage(task.AlertMessages.First(), message);
                task.Enabled = false;
                task.CurrentlyRunning = false;
                await UpdateTask(task);
                return false;
            }
            return false;
        }

        public async Task<List<DiscordEmbed>> CheckSpecificComicAvailability(string handle)
        {
            var discordEmbeds = new List<DiscordEmbed>();
            var targetUrl = _productDetailUrl + handle;
            var payloadJson = await GetJSON(targetUrl);
            var success = !string.IsNullOrWhiteSpace(payloadJson);
            if (success)
            {
                var payloadRoot = JsonSerializer.Deserialize<List<ThirdEyeListingPageDetails>>(payloadJson);
                var payload = payloadRoot?.First() ?? null;
                if (payload != null && (payload.offers?.availability?.Contains("InStock") ?? false) || (payload?.offers?.availability?.Contains("PreOrder") ?? false))
                {
                    var name = payload.name ?? string.Empty;
                    name += " In Stock!";
                    var url = payload.url ?? string.Empty;
                    var description = payload.description ?? string.Empty;
                    var imgUrl = payload.image?.FirstOrDefault() ?? string.Empty;


                    //Cool, we got the guy! let's build an embed.
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = payload.name,
                        Color = DiscordColor.Green,
                    }
                            .WithUrl(url)
                            .WithImageUrl(imgUrl)
                            .WithDescription(description)
                            .Build();
                    discordEmbeds.Add(embed);
                }
            }
            return discordEmbeds;
        }

        

        private static string FixThirdEyeJson(string brokenJson)
        {
            // Fix empty IDs
            brokenJson = Regex.Replace(brokenJson, @"\""id\"":\s*,", "\"id\": null,", RegexOptions.Compiled);

            // Fix empty 'available'
            brokenJson = Regex.Replace(brokenJson, @"\""available\"":\s*,", "\"available\": null,", RegexOptions.Compiled);

            // Fix empty 'first_available_variant' (sometimes it's {id: ,})
            brokenJson = Regex.Replace(brokenJson, @"\""first_available_variant\"":\s*\{[^}]*\}", "\"first_available_variant\": null", RegexOptions.Compiled);

            return brokenJson;
        }


    }
}
