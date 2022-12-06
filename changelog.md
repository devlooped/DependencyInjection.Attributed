# Changelog

## [v1.3.0](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.3.0) (2022-12-06)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v1.2.2...v1.3.0)

:sparkles: Implemented enhancements:

- Make development dependency [\#43](https://github.com/devlooped/DependencyInjection.Attributed/issues/43)

:hammer: Other:

- Never report warning for missing AddServices\(\) in test projects [\#47](https://github.com/devlooped/DependencyInjection.Attributed/issues/47)

:twisted_rightwards_arrows: Merged:

- Don't report warning for missing AddServices\(\) in test projects [\#48](https://github.com/devlooped/DependencyInjection.Attributed/pull/48) (@kzu)

## [v1.2.2](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.2.2) (2022-11-18)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v1.2.1...v1.2.2)

:sparkles: Implemented enhancements:

- Add support for aliased references [\#40](https://github.com/devlooped/DependencyInjection.Attributed/issues/40)
- Add support for aliased references [\#41](https://github.com/devlooped/DependencyInjection.Attributed/pull/41) (@kzu)

:bug: Fixed bugs:

- Don't consider generated code usage of IServiceCollection to report warning [\#37](https://github.com/devlooped/DependencyInjection.Attributed/issues/37)

## [v1.2.1](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.2.1) (2022-11-16)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v1.2.0...v1.2.1)

:sparkles: Implemented enhancements:

- Report warning when using the package without invoking AddServices [\#30](https://github.com/devlooped/DependencyInjection.Attributed/issues/30)

:twisted_rightwards_arrows: Merged:

- Don't consider generated code usage of IServiceCollection to report warning [\#38](https://github.com/devlooped/DependencyInjection.Attributed/pull/38) (@kzu)

## [v1.2.0](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.2.0) (2022-11-10)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v1.1.3...v1.2.0)

:twisted_rightwards_arrows: Merged:

- Report warning when using the package without invoking AddServices [\#31](https://github.com/devlooped/DependencyInjection.Attributed/pull/31) (@kzu)

## [v1.1.3](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.1.3) (2022-10-31)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v1.1.2...v1.1.3)

:sparkles: Implemented enhancements:

- Optimize incremental codegen by only registering for implementation source generation [\#24](https://github.com/devlooped/DependencyInjection.Attributed/issues/24)

## [v1.1.2](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.1.2) (2022-10-03)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v1.1.1...v1.1.2)

## [v1.1.1](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.1.1) (2022-10-03)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v1.1.0...v1.1.1)

:sparkles: Implemented enhancements:

- Add support for MEF attributes [\#17](https://github.com/devlooped/DependencyInjection.Attributed/issues/17)

## [v1.1.0](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.1.0) (2022-09-28)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v1.0.3...v1.1.0)

:sparkles: Implemented enhancements:

- Skip run-time constructor invocation reflection [\#15](https://github.com/devlooped/DependencyInjection.Attributed/issues/15)
- Replace ServiceAttribute.cs inclusion via targets with source generator-based one [\#12](https://github.com/devlooped/DependencyInjection.Attributed/issues/12)

:bug: Fixed bugs:

- Library does not work on MAUI [\#13](https://github.com/devlooped/DependencyInjection.Attributed/issues/13)

:twisted_rightwards_arrows: Merged:

- Add support for MEF attributes [\#18](https://github.com/devlooped/DependencyInjection.Attributed/pull/18) (@kzu)
- Register implementation factory rather than type [\#16](https://github.com/devlooped/DependencyInjection.Attributed/pull/16) (@kzu)
- Replace ServiceAttribute inclusion via targets with source generator [\#14](https://github.com/devlooped/DependencyInjection.Attributed/pull/14) (@kzu)

## [v1.0.3](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.0.3) (2022-09-27)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v1.0.2...v1.0.3)

:hammer: Other:

- Make sure package targets netstandard2.0 [\#10](https://github.com/devlooped/DependencyInjection.Attributed/issues/10)

## [v1.0.2](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.0.2) (2022-09-27)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v1.0.1...v1.0.2)

:sparkles: Implemented enhancements:

- Fix transitive contentFiles support that's missing in nuget [\#8](https://github.com/devlooped/DependencyInjection.Attributed/issues/8)

:twisted_rightwards_arrows: Merged:

- Make sure package targets netstandard2.0 [\#11](https://github.com/devlooped/DependencyInjection.Attributed/pull/11) (@kzu)

## [v1.0.1](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.0.1) (2022-09-27)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v1.0.0...v1.0.1)

:twisted_rightwards_arrows: Merged:

- Fix transitive contentFiles support that's missing in nuget [\#9](https://github.com/devlooped/DependencyInjection.Attributed/pull/9) (@kzu)

## [v1.0.0](https://github.com/devlooped/DependencyInjection.Attributed/tree/v1.0.0) (2022-09-26)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/v0.9.0...v1.0.0)

:sparkles: Implemented enhancements:

- Support automatic registration of covariant implemented interfaces [\#6](https://github.com/devlooped/DependencyInjection.Attributed/issues/6)

:twisted_rightwards_arrows: Merged:

- Support automatic registration of covariant implemented interfaces [\#7](https://github.com/devlooped/DependencyInjection.Attributed/pull/7) (@kzu)

## [v0.9.0](https://github.com/devlooped/DependencyInjection.Attributed/tree/v0.9.0) (2022-09-23)

[Full Changelog](https://github.com/devlooped/DependencyInjection.Attributed/compare/e33ea020586537ad367d7e28fa6503c2f034bf27...v0.9.0)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
