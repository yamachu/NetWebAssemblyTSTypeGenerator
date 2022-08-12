namespace NetWebAssemblyTSTypeGenerator.Tests
{
    public class SplitIntoDictionaryTest
    {
        private Dictionary<string, dynamic> GetBaseDictionary()
        {
            return new Dictionary<string, dynamic>{
                {
                    "hoge", new Dictionary<string, dynamic>{
                        {
                            "fuga", new Dictionary<string, dynamic>{
                                {
                                    "piyo", 1
                                }
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public void EmptyBaseSplitIntoDictionary()
        {
            var result = Utils.SplitIntoDictionary(new Dictionary<string, dynamic>(), ("hoge.fuga.piyo", 1), '.');

            Assert.Equal(GetBaseDictionary(), result);
        }

        [Fact]
        public void MergeSplitIntoDictionary()
        {
            var result = Utils.SplitIntoDictionary(GetBaseDictionary(), ("hoge.fuga.boo", 2), '.');

            Assert.Equal(new Dictionary<string, dynamic>{
                {
                    "hoge", new Dictionary<string, dynamic>{
                        {
                            "fuga", new Dictionary<string, dynamic>{
                                {
                                    "piyo", 1
                                },
                                {
                                    "boo", 2
                                }
                            }
                        }
                    }
                }
            }, result);
        }

        [Fact]
        public void ThrowDuplicateEntrySplitIntoDictionary()
        {
            Assert.Throws<Exception>(() => Utils.SplitIntoDictionary(GetBaseDictionary(), ("hoge.fuga.piyo", 2), '.'));
        }

        [Fact]
        public void ThrowNotDictionaryEntrySplitIntoDictionary()
        {
            Assert.Throws<Exception>(() => Utils.SplitIntoDictionary(GetBaseDictionary(), ("hoge.fuga.piyo.boo", 2), '.'));
        }

        [Fact]
        public void CustomSeparatorSplitIntoDictionary()
        {
            var result = Utils.SplitIntoDictionary(new Dictionary<string, dynamic>(), ("hoge/fuga", 1), '/');

            Assert.Equal(new Dictionary<string, dynamic>{
                {
                    "hoge", new Dictionary<string, dynamic>{
                        {
                            "fuga", 1
                        }
                    }
                }
            }, result);
        }
    }
}
