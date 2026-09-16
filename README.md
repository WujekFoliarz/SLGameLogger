# SLGameLogger

SLGameLogger is an Exiled-based plugin for SCP: Secret Laboratory that records round activity into binary log files for later analysis or replay-style inspection.

It captures player movement, room state, doors, deaths, wave announcements, grenade and projectile events, item pickups, escapes, role changes, and other gameplay events as a round progresses.

## Output format

The plugin creates round log files under:

`Plugins/SLGameLogger/Logs`

Example file name:

`RoundLog 2026-09-16_12-30-45.scpd`

## Configuration

The plugin exposes these settings in `Config`:

- `SavePlayerPositionPerTicks`: controls how often player position snapshots are written
- `FlushDataPerTicks`: determines the flush cadence for queued log data

## Notes

This project is intended for server-side logging for security reasons only

## [PLAYER PROJECT](https://github.com/WujekFoliarz/SLGameLoggerDisplay)
