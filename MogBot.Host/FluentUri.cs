using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using MogBot.Host.Extensions;

namespace MogBot.Host
{
    public class FluentUriBuilder : IUriBuilderStart
    {
        public IUriBuilderOptions WithHost(string host)
        {
            return _builder.WithHost(host);
        }

        public IUriBuilderOptions ParseAbsolute(string uri)
        {
            return _builder.ParseAbsolute(uri);
        }

        public IUriBuilderRelative ParseRelative(string uri)
        {
            return _builder.ParseRelative(uri);
        }

        public static Dictionary<string, string> ParseQuery(string queryParams, bool isEscaped = false)
        {
            var items = queryParams
                .TrimStart('?', ' ')
                .TrimEnd()
                .Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(i =>
                {
                    string key;
                    string value = null;
                    int index = i.IndexOf("=", StringComparison.Ordinal);

                    if (index == -1)
                    {
                        // No =
                        key = i.Trim();
                    }
                    else if (index == i.Length - 1)
                    {
                        // Right side is empty
                        key = i.TrimEnd('=', ' ');
                    }
                    else if (index == 0)
                    {
                        // No key to the left of the =
                        throw new InvalidOperationException("Query string parameters cannot start with =");
                    }
                    else
                    {
                        key = i.Substring(0, index);
                        value = i.Substring(index + 1);
                        if (isEscaped)
                        {
                            value = WebUtility.UrlDecode(value);
                        }
                    }

                    return new {Key = key, Value = value};
                })
                .ToDictionary(i => i.Key, i => i.Value);

            return items;
        }

        private readonly Builder _builder = new Builder();

        private class Builder : IUriBuilderStart, IUriBuilderOptions, IUriBuilderRelative
        {
            public string Host { get; set; }
            public string Scheme { get; set; } = "http";
            public string Path { get; set; }
            public IDictionary<string, string> QueryParams { get; } = new Dictionary<string, string>();
            public int? Port { get; set; }
            public string Fragment { get; set; }

            public IUriBuilderOptions WithScheme(string scheme)
            {
                Scheme = scheme;
                return this;
            }


            public IUriBuilderOptions AddQuery(IDictionary<string, string> queryParams)
            {
                QueryParams.AddRange(queryParams);
                return this;
            }

            public IUriBuilderOptions AddQuery(object queryParams)
            {
                QueryParams.AddRange(queryParams.ToStringDictionary());
                return this;
            }


            public IUriBuilderOptions ParseAndAddQuery(string queryParams, bool isEscaped = false)
            {
                QueryParams.AddRange(ParseQuery(queryParams, isEscaped));
                return this;
            }

            public IUriBuilderOptions AddQuery(string name, object value)
            {
                QueryParams.Add(name, value.ToString());
                return this;
            }

            public IUriBuilderOptions UseSsl()
            {
                Scheme = "https";
                return this;
            }

            public Uri ToUri()
            {
                Validate();
                string uri = Build();
                return new Uri(uri);
            }

            public IUriBuilderOptions WithPort(int port)
            {
                Port = port;
                return this;
            }

            public IUriBuilderOptions WithPath(string path)
            {
                Path = path.Trim('/');
                return this;
            }

            public IUriBuilderOptions WithFragment(string fragment)
            {
                Fragment = fragment.TrimStart('#');
                return this;
            }

            public IUriBuilderOptions RebaseHostTo(Uri newHost)
            {
                WithHost(newHost.Host);
                return this;
            }

            public IUriBuilderOptions WithHost(string host)
            {
                Host = host;
                return this;
            }

            public IUriBuilderOptions ParseAbsolute(string uri)
            {
                // Regex for full URI
                var regex =
                    new Regex(
                        "(?\'scheme\'[^:]+)(?::\\/\\/)(?\'host\'[^:\\/]+)(?::(?\'port\'\\d+))?(?:\\/)?(?\'path\'[^?#]+)?(?:\\/)?(?\'query\'[^#]+)?(?\'fragment\'#.+)?");
                Match match = regex.Match(uri);

                if (!match.Success)
                {
                    throw new InvalidOperationException("Uri is not a valid absolute URI");
                }


                if (match.Groups["scheme"].Success)
                {
                    WithScheme(match.Groups["scheme"].Value);
                }

                if (match.Groups["host"].Success)
                {
                    WithHost(match.Groups["host"].Value);
                }

                if (match.Groups["port"].Success)
                {
                    int port;
                    if (int.TryParse(match.Groups["port"].Value, out port))
                    {
                        WithPort(port);
                    }
                }

                if (match.Groups["path"].Success)
                {
                    WithPath(match.Groups["path"].Value);
                }

                if (match.Groups["query"].Success)
                {
                    ParseAndAddQuery(match.Groups["query"].Value, true);
                }

                if (match.Groups["fragment"].Success)
                {
                    WithFragment(match.Groups["fragment"].Value);
                }

                return this;
            }

            public IUriBuilderRelative ParseRelative(string uri)
            {
                // Regex for path, query and fragement parsing
                var regex = new Regex("(?:\\/)?(?\'path\'[^?#]+)(?:\\/)?(?\'query\'[^#]+)?(?\'fragment\'#.+)?");
                Match match = regex.Match(uri);
                if (!match.Success)
                {
                    throw new InvalidOperationException("Uri is not a valid relative URI");
                }

                if (match.Groups["path"].Success)
                {
                    WithPath(match.Groups["path"].Value);
                }

                if (match.Groups["query"].Success)
                {
                    ParseAndAddQuery(match.Groups["query"].Value, true);
                }

                if (match.Groups["fragment"].Success)
                {
                    WithFragment(match.Groups["fragment"].Value);
                }


                return this;
            }

            private void Validate()
            {
                ValidationResult result = new Validator().Validate(this);

                if (!result.IsValid)
                {
                    throw new InvalidOperationException($"URI Builder validation failed. {result.FormatError()}");
                }
            }

            private bool TryValidate(out string errorMessage)
            {
                ValidationResult result = new Validator().Validate(this);
                errorMessage = result.IsValid ? null : result.FormatError();
                return result.IsValid;
            }

            public override string ToString()
            {
                string error;
                return TryValidate(out error) ? Build() : error;
            }

            private string Build()
            {
                var builder = new StringBuilder();

                builder.Append($"{Scheme}://{Host}");

                if (Port.HasValue)
                {
                    builder.Append($":{Port}");
                }

                builder.Append($"/{Path}");

                if (QueryParams.Count > 0)
                {
                    string query = string.Join("&", QueryParams.Select(i =>
                    {
                        if (i.Value == null)
                        {
                            return $"{i.Key}";
                        }

                        return $"{i.Key}={WebUtility.UrlEncode(i.Value)}";
                    }));

                    builder.Append($"?{query}");
                }

                if (!string.IsNullOrEmpty(Fragment))
                {
                    builder.Append($"#{Fragment}");
                }

                return builder.ToString();
            }
        }

        private class Validator : AbstractValidator<Builder>
        {
            public Validator()
            {
                RuleFor(i => i.Host)
                    .NotNull();

                RuleFor(i => i.Scheme)
                    .NotNull();
            }
        }
    }

    public interface IUriBuilderStart
    {
        IUriBuilderOptions WithHost(string host);
        IUriBuilderOptions ParseAbsolute(string uri);
        IUriBuilderRelative ParseRelative(string uri);
    }

    public interface IUriBuilderRelative
    {
        IUriBuilderOptions WithHost(string host);
        IUriBuilderOptions RebaseHostTo(Uri newHost);
    }

    public interface IUriBuilderOptions
    {
        IUriBuilderOptions AddQuery(IDictionary<string, string> queryParams);
        IUriBuilderOptions AddQuery(object queryParams);
        IUriBuilderOptions AddQuery(string name, object value);
        IUriBuilderOptions ParseAndAddQuery(string queryParams, bool isEscaped = false);
        IUriBuilderOptions UseSsl();
        IUriBuilderOptions WithPort(int port);
        IUriBuilderOptions WithPath(string path);
        IUriBuilderOptions WithScheme(string scheme);
        IUriBuilderOptions WithFragment(string fragment);
        IUriBuilderOptions RebaseHostTo(Uri newHost);
        Uri ToUri();
    }

    public static class FluentUriBuilderExtensions
    {
        public static IUriBuilderOptions UriAbsolute(this string uri)
        {
            var builder = new FluentUriBuilder();
            return builder.ParseAbsolute(uri);
        }

        public static IUriBuilderRelative UriRelative(this string uri)
        {
            var builder = new FluentUriBuilder();
            return builder.ParseRelative(uri);
        }
    }
}