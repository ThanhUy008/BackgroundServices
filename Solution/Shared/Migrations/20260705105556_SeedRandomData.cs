using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Migrations
{
    /// <inheritdoc />
    public partial class SeedRandomData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        migrationBuilder.Sql("""
            INSERT INTO customers (
                id,
                name,
                phone,
                created_on,
                updated_on
            )
            SELECT
                gen_random_uuid(),
                'Customer ' || gs,
                '09' || LPAD((floor(random() * 100000000)::bigint)::text, 8, '0'),
                NOW(),
                NOW()
            FROM generate_series(1, 100) AS gs;
            """);

        migrationBuilder.Sql("""
            INSERT INTO items (
                id,
                name,
                description,
                created_on,
                updated_on
            )
            SELECT
                gen_random_uuid(),
                'Item ' || gs,
                'Description for item ' || gs,
                NOW(),
                NOW()
            FROM generate_series(1, 100) AS gs;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        migrationBuilder.Sql("""
            DELETE FROM customers
            WHERE name LIKE 'Customer %';
            """);

        migrationBuilder.Sql("""
            DELETE FROM items
            WHERE name LIKE 'Item %';
            """);
        }
    }
}
