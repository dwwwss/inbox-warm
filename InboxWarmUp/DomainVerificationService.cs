using DnsClient;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InboxWarmUp
{
    public class DomainVerificationService
    {
        private readonly LookupClient _lookupClient;

        public DomainVerificationService()
        {
            _lookupClient = new LookupClient();
        }

        private string GetDomainFromEmail(string email)
        {
            var parts = email.Split('@');
            return parts.Length == 2 ? parts[1] : null;
        }

        public async Task<bool> VerifySpfAsync(string email)
        {
            var domain = GetDomainFromEmail(email);
            if (domain == null)
            {
                Console.WriteLine($"Invalid email format: {email}");
                return false;
            }

            try
            {
                var result = await _lookupClient.QueryAsync(domain, QueryType.TXT);
                var spfRecords = result.Answers.TxtRecords().Select(x => x.Text).ToList();

                if (!spfRecords.Any())
                {
                    Console.WriteLine($"No SPF records found for {domain}.");
                    return false;
                }

                Console.WriteLine($"SPF Records for {domain}: {string.Join(", ", spfRecords)}");

                var spfRecord = spfRecords.FirstOrDefault(x => x.Contains("v=spf1"));
                return spfRecord != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying SPF record for {domain}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyDmarcAsync(string email)
        {
            var domain = GetDomainFromEmail(email);
            if (domain == null)
            {
                Console.WriteLine($"Invalid email format: {email}");
                return false;
            }

            try
            {
                var result = await _lookupClient.QueryAsync($"_dmarc.{domain}", QueryType.TXT);
                var dmarcRecords = result.Answers.TxtRecords().Select(x => x.Text).ToList();

                if (!dmarcRecords.Any())
                {
                    Console.WriteLine($"No DMARC records found for _dmarc.{domain}.");
                    return false;
                }

                Console.WriteLine($"DMARC Records for _dmarc.{domain}: {string.Join(", ", dmarcRecords)}");

                var dmarcRecord = dmarcRecords.FirstOrDefault(x => x.Contains("v=DMARC1"));
                return dmarcRecord != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying DMARC record for {domain}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyDkimAsync(string email, string selector = "default")
        {
            var domain = GetDomainFromEmail(email);
            if (domain == null)
            {
                Console.WriteLine($"Invalid email format: {email}");
                return false;
            }

            try
            {
                var result = await _lookupClient.QueryAsync($"{selector}._domainkey.{domain}", QueryType.TXT);
                var dkimRecords = result.Answers.TxtRecords().Select(x => x.Text).ToList();

                if (!dkimRecords.Any())
                {
                    Console.WriteLine($"No DKIM records found for {selector}._domainkey.{domain}.");
                    return false;
                }

                Console.WriteLine($"DKIM Records for {selector}._domainkey.{domain}: {string.Join(", ", dkimRecords)}");

                var dkimRecord = dkimRecords.FirstOrDefault(x => x.Contains("v=DKIM1"));
                return dkimRecord != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying DKIM record for {domain}: {ex.Message}");
                return false;
            }
        }
    }
}
