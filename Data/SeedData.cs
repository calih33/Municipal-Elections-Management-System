using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Models;

namespace Municipal_Elections_Management_System.Data;

public static class SeedData
{
    // this is an extension method to the ModelBuilder class
    public static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Municipality>().HasData(
            GetMunicipalities()
        );
        modelBuilder.Entity<Candidate>().HasData(
            GetCandidates()
        );
        modelBuilder.Entity<Position>().HasData(
            GetPositions()
        );
    }
    public static List<Municipality> GetMunicipalities()
    {
        List<Municipality> municipalities = new List<Municipality>() {
            new Municipality() {
                MunicipalityId=1,
                Name="Richmond",
                ElectionDate= DateTime.Parse("2026-10-17"),
                Description="Richmond is a dynamic and ethnically diverse urban center, blending residential, commercial, agricultural, and industrial spaces. It’s a city where the Fraser River meets the Pacific Ocean, offering an ideal environment for business, nature, and community life.",
                LogoImage="richmond_logo.png"
            },
        };
        return municipalities;
    }

    public static List<Candidate> GetCandidates()
    {
        List<Candidate> candidates = new List<Candidate>() {
            new Candidate()
            {
                CandidateId=1,
                FirstName="Dickens",
                LastName="Cheung",
                Position="Mayor",
                Description="As a UBC Sauder School of Business graduate specializing in Finance, I've spent 15 years leading and growing a local herbal tea manufacturing operation, navigating complex food safety, labelling, and import/export regulations while ensuring full compliance with all three levels of government. But my most important role is father to four school-aged children, all born and raised right here in Richmond, because I'm doing this for their future. I firmly believe that the people are the masters, and elected officials are their servants, not the other way around. That's why I personally practice servant leadership: leadership built on humility, accountability, and listening first. City Hall should never tell residents what to accept; it should listen, respond, and serve. I'm running because Richmond belongs to all of us: the parents, workers, seniors, and students who call it home. Together, we thrive. We Are Richmond. Vote Dickens CHEUNG for Mayor.",
                URL="https://www.wearerichmond.ca/",
                Image="dickens_cheung.png",
                PositionId=1
            },
        }
        ;
        return candidates;
    }

    public static List<Position> GetPositions()
    {
        List<Position> positions = new List<Position>() {
            new Position()
            {
                PositionId=1,
                Type=Position.PositionType.Mayor,
                MunicipalityId=1,
                Description="The Mayor is the head of the municipal government, responsible for leading the council, representing the municipality, and ensuring the effective administration of local policies and services.",
                NumberOfPositions=1

            },
        };
        return positions;
    }
}