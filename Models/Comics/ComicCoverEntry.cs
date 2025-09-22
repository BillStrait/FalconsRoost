using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models.Comics
{
    public class CoverContest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public WeeklyVoteStatus Status { get; set; } = WeeklyVoteStatus.Nominations;

        public ICollection<CoverContestCover> Covers { get; set; } = new List<CoverContestCover>();
        public ICollection<CoverContestVote> Votes { get; set; } = new List<CoverContestVote>();
    }

    public enum WeeklyVoteStatus
    {
        Nominations,
        Voting,
        Closed
    }

    public class CoverContestCover
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WeeklyContestId { get; set; }
        public CoverContest WeeklyContest { get; set; } = null!;

        [MaxLength(100)]
        public string LocgId { get; set; } = string.Empty; // League of Comic Geeks ID

        public string Title { get; set; } = string.Empty;
        public string IssueNumber { get; set; } = string.Empty;
        public ulong SubmittedByUserId { get; set; } // Discord snowflake

        public string ImagePath { get; set; } = string.Empty;
        public float[]? VectorEmbedding { get; set; } // store raw JSON/array for now

        public ICollection<CoverContestVote> Votes { get; set; } = new List<CoverContestVote>();
    }

    public class CoverContestVote
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WeeklyContestId { get; set; }
        public CoverContest WeeklyContest { get; set; } = null!;

        public Guid CoverId { get; set; }
        public CoverContestCover Cover { get; set; } = null!;

        public ulong UserId { get; set; } // Discord snowflake
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
