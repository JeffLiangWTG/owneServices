using System;

namespace Enterprise.Customs.PL.Business.Testing;

static class TestPkcs12Certificate
{
	const string Base64Value = """
								MIILXwIBAzCCCxUGCSqGSIb3DQEHAaCCCwYEggsCMIIK/jCCBWoGCSqGSIb3DQEH
								BqCCBVswggVXAgEAMIIFUAYJKoZIhvcNAQcBMF8GCSqGSIb3DQEFDTBSMDEGCSqG
								SIb3DQEFDDAkBBB/uNQxsbi3B9wVY0YsbJdXAgIIADAMBggqhkiG9w0CCQUAMB0G
								CWCGSAFlAwQBKgQQNv5HZ4RshYiBvJXoURsuh4CCBOCNk9lZzXYMrjl7v9ZmW8DV
								i1h+kHhblhMET2sgDxoQT6gu8YIOch4nyqESRS0iJ5xA4ccJM7AK9Ul2y2Nmlkz5
								g3KDeO/bq7vJQVvMsQHOuTJ+EgBwW83bCzdQ/nnIkTx2Hz6giCeSbCqB7sU0hcSb
								G6Dynhco1sE1/Q436FoisofPe2Towrbyamxbp3xhkchHqDdjjjW/n+siU1D1t8uz
								0b6DMVLHxKxfilXL/7NtADzckMJVSakWuQ89ewRxEGvIcJOBl4+ciag4dkfxIDWA
								WIni3Fuvfn+eF6Udwe+hOdMZTV59T9LdgGlPV/gWvXZqdDwxHQayaiP4Ni7ar924
								aDSu7lXwe7r2n7y2L/Wr+PQ+KMUhKa01K9gZmkZDG6asRP4PwcboqV6ILvEAj0Mu
								AMosQwncfJidzr84PGRVhj3NMU4s9V93kXtkwI6YhyUpNzPiprDg8ED1gjFYtVtV
								VTYJyCcRuEq0YhumGCvI+KpUMEc3v1XliJGnT+cIfSYRZhjldY5ucCtP0UecKdqe
								bm8LbLzxfHdNwtLDjBen9Yd1UwAViyDWpGJ9sArnXdtLCmxHwHaTiWIcA16Dftg3
								Lqj12EqP5Dr51AU2M1N9tLj470qFDlIyo+UFo+S1XnxhqW5wTsGw24TnwrmG10+F
								dsGYV0PNtQ6h7+9zePEh5kOUn6ca9GnS+wDsJnTzL669lwzY8zoG3BdgZbBCj20q
								h7VBuQ8ET0iRuYDRJybeiKssfHfWNxk8Q+rsRz3VJZvV7xZixKYj9HxTOFqtzBrW
								b2uH6OLXi6T+nmoA8dfEDKz97bDnfJRfvOmGIbkyaxiQyNDKenG7FArEkJseZF9V
								MA81DJ2srofFIGkU8ZIVAFLmzqyGXhG1QSMZKC23kj9SJfMff6ynd4iIr+PQN+SS
								CezjU6WAdUUOAbDN3ZM4rSUfGOwSrpQT4eWPMKTy+n5A1EFeVVOW4bNWBhys2gCL
								VsbSsBHKONs1Gk42+dxqzj6fcnewOfniq3Aw2+1OE9NFvmAcbWSlo2HPTU+NoZBx
								w30Ggf4K4037jZUC96bk0RHHTrohXAgQ5kSumpSnhpLIhIugdajI7ftSAFsgrrCE
								s3FeWePKALkWHDKgo2irBipfmBJ32jjqr0Uw6Y4+7QXtBOfcCJ0pjHP9VkE3AeXE
								Uo3QaQpI+YitDpgD+BM43QVXVOQFVfqzQ6PtCeY8Rrft2kyLDfxEZRCOvnwAWdsm
								p4uF3PojkVLpX9UCf2+n199bhlVD2y8id49UkxEXFj4uopX5d97I07KEOn67togW
								RzJZzVODKIIlJ1BRNHPhVud54L8JJ5gbnpqkgJugVMdgcTlNA6g7YC+YA/PZp6mN
								sIaPfP28Mkl0m12x6wkb5VlD4Be78jbOVkXeKAhAnKHB+OS6d+dgGaEK5X11mNT8
								feC+nV6AD4cT5Xm8YG5t/Cza2TBtQgILYveSgGVJ7MuCrhk+kAw3PDyziw350Udx
								JgEJ2LnT0ZiNkqXixgck5l1ZUYaQ5o3RXpl5CFpXyHQAAIx8LQzzAhucD+/g1SZN
								Tcxo6VxDUFGyrrjG3SZGVldZonENYFRkNAzV4Y3YEF1KKfuUHHovuzSSI31iYYQG
								VeA0EucCGihUtY4RUqywl9anG4gAI8YahkBC3h+J9c8wggWMBgkqhkiG9w0BBwGg
								ggV9BIIFeTCCBXUwggVxBgsqhkiG9w0BDAoBAqCCBTkwggU1MF8GCSqGSIb3DQEF
								DTBSMDEGCSqGSIb3DQEFDDAkBBCAbXbEFpgId+uCeY8kgknPAgIIADAMBggqhkiG
								9w0CCQUAMB0GCWCGSAFlAwQBKgQQ84dR5LP6GbOKeFNsdK3w4QSCBNBZcMR+fxQ8
								56TWvYdizXpvuXlbjgw/lIueEHEau2BXVQddmuSSWZVwWu0URAhnvJGN+toZhCFX
								47Qo6TmuArB48AOtlUwg9km7HK66JfN+jSEJmMPOwbrI+XdR61CgsrrA9tCwX2wS
								gb0KjWk0WX8zzQPvbntqLdduurwm/7KHcJdACWwDCJ3YM4ZYxrwBx6AKeK9/dcsE
								uoiOlqOcjOSz7GSRKbyHULwm18C6GtD5YCKCkrU2hS+da9cP1QZYS15uOZXylRvA
								RFT+mdH48g9Ad5usEYzAutGYPubhA0y7Nd3wCqMvE4S2cojps23hME1d5BUvQ+F8
								4d1ecBXTJIDnWwXPqq0lJtj+TTnjr2S3ddgkZZXrHEAAoubSLRISix7/tdVvptla
								ifxMFaBAR3ztrl+n2VSYGp6Irjmc66D3ijsoX58MBFEo8t84uK6Di9hGVN0croPV
								EUTb1OD12nE0AdH2/BBwOBmkLZ1rXCV1CMlWa2NiTngBcPQN0g3bklkqn3nAHDI2
								JsWqGQplVehR+TL8pU8U4QSKGZVUQmktS1iHXS0r/qaH9waDZWrSvzma95t+cvJs
								4cAc74cPbvp/foyIuwjNxALjgfVAOzkHTrBJBZ+zuOV+lGsAa78Wy8RoetLM/hgI
								PGjg0xCTSXdNWCGKHpOxdyE0NM9981sAKOJD+5HFGajRRyh75XLz0n/8FLOPc9jT
								zEbi562yB8DUxBrEDM+8g4fRbCc82o/VoAlIZfAfiAblEWSV8tuyT+uMPOtwFsRX
								/YnMKafys57najCZZ1Lkdf99i9dI4kByJ2Lzdig8Y+DnYcAb0CUgwwsiQOtEuWYl
								mxfO67rlHNJVgWYBMAxsgrD2NQcnp1V0sJ+Q0baa4v+ljTqNGq8DV6HnozagswY1
								OPkJCPBs1e5oLPo6l484Dwa7tBAThBRmJJvPK8S+NkIW9V71eLQ/jm33368k41YV
								x44NHRTdKsyzASaUBGtys3rq4UuefwbVOs2b+UFpoKFy0tfCe78/ufG2sNk178K3
								eWvIycnvjOcBFR+Y5gKKxCR842xfpg+o0Y291+UuTtoXWyS60mfeM3fIyu6Zyf4B
								dEKjBvryCb0hO38OSwyOpp83rwqA7MYoIGayatrR8UUaefK+TqqhEZTvccYWy0Mz
								jl+a6F+3zQ7Gq0goyagedqGZoIhYhOAwqPEGVqAA5zat6ikmgZsVSNzn9Dc0Xuk2
								cQEUxlo5gAl6agVc0f0fvJfNp0uEaVrYlFYjviNS5UPedUycH89vg1WPPNIlGK7t
								uuedN254s0y6wDnErAObNqjHLqK8INaU29IHaI4aXWlpO2AZEK6oa4Oow+GEA/Yi
								ID0qTw7hxWyNwx+8EaU002Dx5txAyqKGXAXUozNvGbu5QOW2Y/yfrs9cRSSHodXw
								8/pKswTjJt8G0894j2W/WP0YeBuJFIKa6n5mOVy5yfDoffLcKsLKlfj/Hs0Iwo5M
								uH5xDw/AgylcgFJWNUjO7ittgxMh6RylYkRzy9d+Pv5qcYh/78uk4Fw8ZLy1NCbc
								nl9ZpPkeGbEnn75VQarR2x0rqQdso/z32Wrnq61w/pltanjwK7eXTzRfgAFFgg2U
								hyDescFs+sZNRKQNuAG0a1h248VrJlguJTElMCMGCSqGSIb3DQEJFTEWBBRSt2nU
								K3GN/MJEF0aPAV6wDfmUTTBBMDEwDQYJYIZIAWUDBAIBBQAEIMangzV3Wy+nKIYo
								zmtCYlHZCNSCpFLANxIUMMEGJJ+pBAiuqLqA2yOgpAICCAA=

								""";

	public const string Password = "passw0rd";

	public static byte[] Value => Convert.FromBase64String(Base64Value);
}
