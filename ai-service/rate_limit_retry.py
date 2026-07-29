import time
from collections.abc import Callable
from typing import TypeVar

import voyageai

T = TypeVar("T")

MAX_RETRIES = 2
# Voyage'ın rate limit penceresi dakikalık; pencerenin kesin olarak
# resetlenmesi için 60sn'nin biraz üstünde bekliyoruz.
WAIT_SECONDS = 65


def with_rate_limit_retry(func: Callable[[], T]) -> T:
    for attempt in range(MAX_RETRIES + 1):
        try:
            return func()
        except voyageai.error.RateLimitError:
            if attempt == MAX_RETRIES:
                raise
            time.sleep(WAIT_SECONDS)
    raise AssertionError("unreachable")
