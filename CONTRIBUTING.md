# Contributing to Trade Template MVP

Thanks for your interest in contributing! This project welcomes contributions from traders, developers, and anyone interested in algorithmic trading systems.

## 🚨 Important Note

**This is experimental trading software.** All contributors should understand:
- Trading involves substantial risk of loss
- Past performance doesn't guarantee future results  
- Always test thoroughly on simulation before risking capital
- Contributors are not responsible for trading losses

## Getting Started

### Prerequisites

- **NinjaTrader 8**: Required for strategy testing
- **Basic C# Knowledge**: For strategy modifications
- **Trading Experience**: Understanding of EMA indicators and risk management
- **Git/GitHub**: For version control and collaboration

### Development Environment

1. **Fork the Repository**
   ```bash
   git clone https://github.com/ol-s-cloud/agnes-bot
   cd trade-template-mvp
   ```

2. **NinjaTrader Setup**
   - Import `nt/TradeTemplateEMA.cs` into NinjaScript Editor
   - Test on Sim101 account initially
   - Use small position sizes during development

3. **Web Interface Testing**
   - Open `web/index.html` in browser locally
   - Or deploy to Vercel for testing

## Contribution Areas

### 🎯 High Priority

**Trading Logic Enhancements:**
- Additional technical indicators (RSI, MACD, Bollinger Bands)
- Multi-timeframe analysis support
- Advanced entry/exit conditions
- Portfolio-level risk management

**Risk Management:**
- Daily/weekly loss limits
- Dynamic position sizing
- Correlation-based filters
- Volatility-adjusted stops

**Performance & Monitoring:**
- Real-time metrics dashboard
- Trade logging and analysis
- Backtesting integration
- Performance attribution

### 🔧 Medium Priority

**Configuration System:**
- Database-backed config (vs. file-based)
- User authentication and profiles
- Strategy templates and presets
- A/B testing framework

**Integration & APIs:**
- Webhook notifications (Discord, Slack, etc.)
- Third-party data sources
- Portfolio management systems
- Compliance reporting tools

**User Experience:**
- Mobile-friendly web interface
- Strategy wizard/builder
- Parameter optimization tools
- Educational documentation

### 📝 Documentation & Testing

- Code documentation and examples
- Video tutorials and guides
- Unit tests for strategy logic
- Integration testing frameworks
- Performance benchmarking

## Code Standards

### C# Strategy Code

**Style Guidelines:**
- Use PascalCase for public members
- Use camelCase for private fields
- Add XML documentation for public methods
- Follow NinjaTrader coding conventions

**Example:**
```csharp
/// <summary>
/// Validates configuration parameters against defined bounds
/// </summary>
/// <param name="config">Configuration dictionary to validate</param>
/// <returns>True if all parameters are within valid ranges</returns>
private bool ValidateConfiguration(Dictionary<string, string> config)
{
    // Implementation here
}
```

**Performance Considerations:**
- Minimize object allocation in OnBarUpdate()
- Use efficient data structures for indicators
- Avoid network calls in trading loops
- Cache frequently accessed values

### Configuration Files

**Format:**
- Use key=value pairs with lowercase keys
- Include descriptive comments
- Group related parameters logically
- Validate all user inputs

**Example:**
```
# Strategy Parameters
ema_fast=10          # Fast EMA period for trend detection
ema_mid=50           # Medium EMA for initial signal
ema_slow=200         # Slow EMA for confirmation
```

### Web Interface

**HTML/CSS/JavaScript:**
- Use semantic HTML elements
- Responsive design for mobile compatibility
- Vanilla JavaScript (no frameworks for simplicity)
- Clear form validation and error messages

## Submission Process

### 1. Issue First

Before starting work:
- Check existing issues to avoid duplication
- Open a new issue describing the feature/bug
- Discuss approach with maintainers
- Get approval for significant changes

### 2. Development Workflow

```bash
# Create feature branch
git checkout -b feature/your-feature-name

# Make changes and commit
git add .
git commit -m "Add: Brief description of changes"

# Push and create PR
git push origin feature/your-feature-name
```

### 3. Pull Request Guidelines

**Required:**
- Clear description of changes
- Testing instructions
- Screenshots for UI changes
- Sim account testing results
- Breaking change documentation

**PR Template:**
```markdown
## Description
Brief summary of changes

## Testing
- [ ] Tested on Sim101 account
- [ ] Configuration validation works
- [ ] No runtime errors in NinjaTrader Output

## Screenshots (if applicable)
[Add screenshots here]

## Breaking Changes
List any breaking changes and migration steps
```

### 4. Code Review Process

**Reviewers will check:**
- Trading logic correctness
- Risk management considerations
- Code quality and performance
- Documentation completeness
- Testing coverage

**Review Criteria:**
- Does it improve the trading system?
- Is it properly tested?
- Are risks clearly documented?
- Is the code maintainable?

## Testing Requirements

### Trading Strategy Testing

**Minimum Testing:**
1. **Sim Account Testing**: All changes must work on simulation
2. **Parameter Validation**: Test boundary conditions and invalid inputs  
3. **Configuration Changes**: Verify hot-reload functionality
4. **Error Handling**: Test network failures and file errors

**Recommended Testing:**
1. **Multiple Timeframes**: Test on 1m, 5m, 15m charts
2. **Different Markets**: Test on ES, NQ, other futures
3. **Market Conditions**: Test in trending and ranging markets
4. **Edge Cases**: Test with extreme parameter values

### Configuration Testing

**Web Interface:**
- Test all form inputs and validations
- Verify copy/paste functionality
- Check mobile responsiveness
- Test with different browsers

**File System:**
- Test GitHub config updates
- Test local file overrides
- Test Last-Known-Good recovery
- Test invalid configurations

## Security Considerations

### Never Commit:
- Real API keys or passwords
- Live trading account credentials
- Personal trading data or results
- Proprietary trading signals

### Best Practices:
- Use placeholder values in examples
- Document security implications of changes
- Test with minimal permissions
- Validate all external inputs

## Communication

### Channels
- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: General questions and ideas
- **Pull Request Comments**: Code-specific discussions

### Guidelines
- Be respectful and constructive
- Focus on the code, not the person
- Share trading results responsibly
- Help newcomers get started

## Legal and Ethical Considerations

### Disclaimers
- All contributors acknowledge trading risks
- No guarantees of profitability
- Educational purpose emphasis
- Compliance with local regulations

### Code of Conduct
- Professional and respectful communication
- No financial advice or recommendations
- No pump/dump or market manipulation discussions
- Focus on technical implementation

## Recognition

Contributors will be:
- Listed in project README
- Credited in release notes for significant contributions
- Mentioned in documentation they author

---

## Quick Start Checklist

- [ ] Read this entire document
- [ ] Set up development environment
- [ ] Test current codebase on simulation
- [ ] Choose a contribution area
- [ ] Open an issue to discuss your ideas
- [ ] Fork and create feature branch
- [ ] Make changes with proper testing
- [ ] Submit pull request with clear description

---

## Author & Contact

**Authored by:** Olar Sa'id  
**GitHub:** [@ol-s-cloud](https://github.com/ol-s-cloud)  
**Repository:** [AGNES-BOT](https://github.com/ol-s-cloud/agnes-bot)  

*If this project helped you, please star the repository and consider contributing!*

**Disclaimer:** Educational software only. Trading involves substantial risk. Always test thoroughly on simulation accounts before live trading.


**Remember: Always prioritize risk management and thorough testing in a simulation environment before any live trading application.**

Thanks for contributing to making algorithmic trading more accessible and safer for everyone!
