# SLGameLogger

SLGameLogger is an Exiled-based plugin for SCP: Secret Laboratory that records round activity into binary log files for later analysis or replay-style inspection.

It captures player movement, room state, doors, deaths, wave announcements, grenade and projectile events, item pickups, escapes, role changes, and other gameplay events as a round progresses.

## Output format

The plugin creates round log files under:

`Plugins/SLGameLogger/Logs`

## Configuration

The plugin exposes these settings in `Config`:

- `SavePlayerPositionPerTicks`: controls how often player position snapshots are written
- `FlushDataPerTicks`: determines the flush cadence for queued log data


# Setup

- Go to config (/EXILED/Configs/Plugins/s_l_game_logger/)
- Set **listen_host** to ``*``
- Set **port** to whatever you want
- Set **motd** to what should appear in the server list

# Making the server appear 
- Open an issue in this [repo](https://github.com/WujekFoliarz/SLGameLoggerDisplay) with the http address or add it to this [file](https://github.com/WujekFoliarz/SLGameLoggerDisplay/blob/main/ServerList.txt) and create a pull request
- You can also add me on discord (wujek_foliarz)

## Notes

This project is intended for server-side logging for security reasons only

## [PLAYER PROJECT](https://github.com/WujekFoliarz/SLGameLoggerDisplay)
