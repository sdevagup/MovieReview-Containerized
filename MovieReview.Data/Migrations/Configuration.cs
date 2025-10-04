namespace MovieReview.Data.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<MovieReviewDbContext>
    {
        public Configuration()
        {
            // Allow EF to auto-create tables if they don't exist
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(MovieReviewDbContext context)
    }
}
