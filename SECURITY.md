# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Security Considerations for Trading Software

This project involves financial trading software that handles potentially sensitive information and connects to trading platforms. Security is paramount.

### High-Risk Areas

**Configuration Files:**
- Never commit real API keys, passwords, or account credentials
- Use local file overrides for sensitive parameters
- Validate all configuration inputs to prevent injection attacks

**Network Communications:**
- GitHub config polling uses HTTPS only
- WebClient implementation validates URLs
- No credentials transmitted over public connections

**File System Access:**
- Strategy writes to user's Documents folder only
- Path validation prevents directory traversal
- Local config files should have restricted permissions

## Reporting Security Vulnerabilities

**Please DO NOT report security vulnerabilities through public GitHub issues.**

Instead, please report them privately using one of these methods:

### Preferred Method: Security Advisory
1. Go to the repository's Security tab
2. Click "Report a vulnerability"
3. Fill out the private security advisory form

### Alternative Method: Email
Send an email to: [gs_wl889@icloud.com] with:
- Subject: "Security Vulnerability - Trade Template MVP"
- Detailed description of the vulnerability
- Steps to reproduce the issue
- Potential impact assessment
- Suggested fixes (if any)

### Response Time
- **Initial Response**: Within 48 hours
- **Preliminary Assessment**: Within 7 days
- **Fix Timeline**: Varies by severity (see below)

## Vulnerability Severity Levels

### Critical (Fix within 24-48 hours)
- Remote code execution vulnerabilities
- Unauthorized trading account access
- Credential exposure or theft
- Financial data manipulation

### High (Fix within 1 week)
- Local privilege escalation
- Configuration injection attacks
- Denial of service affecting trading
- Sensitive information disclosure

### Medium (Fix within 1 month)
- Cross-site scripting in web interface
- Local information disclosure
- Input validation bypasses
- Insecure default configurations

### Low (Fix in next release)
- Information leakage (non-sensitive)
- Minor security misconfigurations
- Best practice improvements

## Security Best Practices for Users

### Configuration Security
- **Never commit real credentials to Git repositories**
- Use local configuration files for sensitive parameters
- Restrict file permissions on local config files
- Regularly rotate API keys and passwords

### Network Security
- Use HTTPS for all GitHub config URLs
- Avoid public WiFi for trading activities
- Keep NinjaTrader and Windows updated
- Use reputable antivirus software

### Trading Environment
- Use separate accounts for simulation and live trading
- Start with minimal position sizes when testing
- Monitor account activity regularly
- Enable account notifications and alerts

### Development Security
- Test all changes on simulation accounts first
- Validate user inputs in web interface
- Use secure coding practices for C#
- Review third-party dependencies for vulnerabilities

## Known Security Considerations

### By Design Limitations
1. **Public Configuration Files**: Basic parameters are stored in public GitHub repositories
2. **Network Polling**: Strategy makes HTTP requests to GitHub every 5 seconds
3. **Local File Access**: Strategy reads/writes to user's Documents folder

### Mitigation Strategies
1. **Local Overrides**: Sensitive parameters can be stored in private local files
2. **HTTPS Only**: All network communications use encrypted connections
3. **Restricted Permissions**: File operations limited to specific directories

### Current Security Measures

**Input Validation:**
- Configuration parameter bounds checking
- File path validation to prevent directory traversal
- Network URL validation for GitHub endpoints

**Error Handling:**
- Network failures fail silently (no sensitive info in logs)
- File errors don't expose system information
- Graceful fallbacks prevent strategy crashes

**Access Control:**
- Account mode enforcement prevents demo/live mix-ups
- Local configuration files override public settings
- No remote command execution capabilities

## Responsible Disclosure

### What We Ask
- Allow reasonable time for fixes before public disclosure
- Provide clear reproduction steps
- Suggest potential mitigations if known
- Avoid testing on live trading accounts

### What We Promise
- Timely response to security reports
- Credit for responsible disclosure (if desired)
- Regular security updates and patches
- Transparent communication about fixes

### Public Disclosure Timeline
- **After Fix**: Security issues will be publicly disclosed 90 days after resolution
- **Coordination**: We'll work with reporters on disclosure timing
- **CVE Assignment**: Critical vulnerabilities may receive CVE identifiers

## Security Updates

Users will be notified of security updates through:
- GitHub Security Advisories
- Repository README updates
- Release notes with security tags
- Email notifications (if contact provided)

## Compliance and Regulations

### Financial Regulations
This software may be subject to:
- CFTC regulations for automated trading
- SEC requirements for investment advisors
- Local financial services regulations

**User Responsibility:**
Users must ensure compliance with applicable regulations in their jurisdiction.

### Data Protection
- No personal trading data is collected by this software
- Configuration files may contain trading parameters
- Users responsible for protecting their own data

## Security Audit History

| Date | Scope | Findings | Status |
|------|-------|----------|--------|
| 2025-01-09 | Initial Release | Low: Default config exposure | Documented |

## Contact Information

For security-related questions or concerns:
- **Security Issues**: Use GitHub Security Advisory
- **General Security Questions**: Open a GitHub Discussion tagged "security"

---

**Remember**: This software handles financial transactions. Always prioritize security over convenience, and never trade with funds you cannot afford to lose.
