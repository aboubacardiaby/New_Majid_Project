# Gambian Muslim Community Website

A modern ASP.NET Core MVC website for the Gambian Muslim Community in Minnesota, featuring Islamic-themed design and database-driven content management.

## Features

- **Responsive Design**: Mobile-friendly Islamic-themed interface
- **Database-Driven**: Entity Framework Core with SQL Server
- **Community Services**: Prayer services, education, support, events
- **Prayer Times**: Dynamic prayer schedule for Minneapolis
- **Event Management**: Community events and programs
- **Contact System**: Contact form with database storage
- **Admin-Ready**: Easily extensible for admin functionality

## Technology Stack

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Frontend**: HTML5, CSS3, JavaScript, Font Awesome
- **Fonts**: Google Fonts (Amiri, Open Sans)

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd GambianMuslimCommunity
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection** (if needed)
   - Edit `appsettings.json`
   - Modify the `DefaultConnection` string

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the website**
   - Open browser to `https://localhost:7000` or `http://localhost:5000`

### Database Setup

The application uses Entity Framework Code-First approach with automatic database creation:

- **Models**: Services, Events, Prayer Times, Contact Messages
- **Seed Data**: Initial services and prayer times are automatically seeded
- **Migrations**: Run automatically on first startup

### Project Structure

```
GambianMuslimCommunity/
├── Controllers/           # MVC Controllers
├── Models/               # Data models and ViewModels
├── Views/                # Razor views
├── Services/             # Business logic services
├── Data/                 # Entity Framework DbContext
├── wwwroot/              # Static files (CSS, JS, images)
├── Properties/           # Launch settings
└── Migrations/           # EF Core migrations
```

## Features in Detail

### Prayer Times
- Dynamic prayer schedule for Minneapolis, MN
- Next prayer highlighting
- Time format with AM/PM
- Database-driven for easy updates

### Services
- Prayer Services
- Islamic Education
- Community Support
- Cultural Events
- Marriage & Family Services
- Zakat & Charity Programs

### Events Management
- Upcoming community events
- Event details with date, time, location
- Categorized by type (Religious, Educational, Youth)

### Contact System
- Contact form with validation
- Database storage of messages
- Success/error handling
- Admin notification logging

## Customization

### Adding New Content
1. Use Entity Framework to manage database content
2. Extend models in `Models/` folder
3. Update `ApplicationDbContext` for new entities
4. Create migrations: `dotnet ef migrations add <MigrationName>`

### Styling
- Main CSS: `wwwroot/css/site.css`
- Islamic color scheme with green and gold
- Responsive grid layouts
- Font Awesome icons

### Prayer Times
- Stored in database for easy management
- Can be updated daily/monthly via admin interface
- Automatic calculation of next prayer

## Database Schema

### Services Table
- Id, Title, Description, IconClass, IsActive

### CommunityEvents Table
- Id, Title, Description, EventDate, StartTime, EndTime, Location, EventType

### PrayerTimes Table
- Id, Name, Time, Date, City, IsActive

### ContactMessages Table
- Id, Name, Email, Subject, Message, DateSent

## Deployment

### Development
```bash
dotnet run
```

### Production
```bash
dotnet publish -c Release
```

Deploy to IIS, Azure App Service, or other hosting platforms.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes following Islamic community guidelines
4. Test thoroughly
5. Submit a pull request

## License

This project is created for the Gambian Muslim Community in Minnesota.

## Support

For technical support or community inquiries, contact the development team or community administrators.

---

**Built with ❤️ for the Gambian Muslim Community in Minnesota**

*May Allah (SWT) bless this project and the community it serves. Ameen.*