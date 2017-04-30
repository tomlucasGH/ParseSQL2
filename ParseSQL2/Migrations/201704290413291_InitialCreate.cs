namespace ParseSQL2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QueryMasters",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        QueryText = c.String(),
                        customerid = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.QueryMasters");
        }
    }
}
